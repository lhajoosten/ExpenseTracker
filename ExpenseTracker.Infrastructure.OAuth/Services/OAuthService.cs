using ExpenseTracker.Common.Abstractions.OAuth;
using ExpenseTracker.Common.Models.OAuth;
using ExpenseTracker.Infrastructure.Identity.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace ExpenseTracker.Infrastructure.OAuth.Services
{
    public class OAuthService(
        SignInManager<AppUser> signInManager,
        UserManager<AppUser> userManager,
        IOAuthProviderService providerService,
        IOAuthUserService userService,
        IConfiguration configuration,
        ILogger<OAuthService> logger) : IOAuthService
    {
        private readonly SignInManager<AppUser> _signInManager = signInManager;
        private readonly UserManager<AppUser> _userManager = userManager;
        private readonly IOAuthProviderService _providerService = providerService;
        private readonly IOAuthUserService _userService = userService;
        private readonly IConfiguration _configuration = configuration;
        private readonly ILogger<OAuthService?> _logger = logger;

        public async Task<AuthenticationProperties> ConfigureExternalAuthenticationAsync(string provider, string returnUrl)
        {
            try
            {
                _logger.LogInformation("Configuring external authentication for provider: {Provider} with returnUrl: {ReturnUrl}",
                    provider, returnUrl);

                string normalizedProvider = _providerService.NormalizeProviderName(provider);

                var properties = new AuthenticationProperties
                {
                    RedirectUri = $"/api/oauth/{normalizedProvider.ToLower()}-callback",
                    AllowRefresh = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10),
                    IsPersistent = true
                };

                properties.Items["returnUrl"] = returnUrl ?? _configuration["App:ClientUrl"];

                return await Task.FromResult(properties);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error configuring external authentication");
                throw;
            }
        }

        public async Task<OAuthLoginResult?> ProcessCallbackAsync(string provider, HttpContext httpContext, string clientUrl)
        {
            try
            {
                _logger.LogInformation("Processing {Provider} callback", provider);

                // We need to pass the httpContext to the SignInManager to get the external login info
                var info = await _signInManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    _logger.LogWarning("No external login info found for {Provider}", provider);
                    return OAuthLoginResult.Failure(
                        $"{clientUrl}?error=authentication_failed&provider={provider}&message=no_login_info",
                        "Authentication failed",
                        "No login information found");
                }

                _logger.LogInformation("External login info retrieved successfully for {Provider}", provider);

                return await ProcessExternalLoginAsync(info, clientUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing {Provider} callback", provider);
                return OAuthLoginResult.Failure(
                    $"{clientUrl}?error=unknown&message={Uri.EscapeDataString(ex.Message)}",
                    "Unknown error",
                    ex.Message);
            }
        }

        public async Task<OAuthLoginResult> HandleCallbackWithoutParamsAsync(ClaimsPrincipal user, string provider, string clientUrl)
        {
            _logger.LogInformation("Handling callback without parameters for {Provider}", provider);

            if (user.Identity?.IsAuthenticated == true)
            {
                _logger.LogInformation("User is already authenticated, redirecting to client");
                return await Task.FromResult(OAuthLoginResult.Success(clientUrl, null!));
            }

            _logger.LogWarning("{Provider}Callback called without code or state parameters", provider);
            return await Task.FromResult(OAuthLoginResult.Failure(
                $"{clientUrl}?error=no_params&provider={provider}",
                "No authentication parameters",
                "Callback called without required parameters"));
        }

        public async Task<OAuthLoginResult> HandleStateFailureAsync(ClaimsPrincipal principal, string provider, string error)
        {
            _logger.LogInformation("Handling state validation failure for {Provider} with error: {Error}", provider, error);

            // Only respond with success if the user is already authenticated
            if (principal.Identity?.IsAuthenticated == true)
            {
                _logger.LogInformation("User is already authenticated despite state error, redirecting to success");

                // Check if user exists in database
                var userResult = await _userService.GetCurrentUserStatusAsync(principal);
                if (!userResult.IsSuccess || userResult.User == null)
                {
                    _logger.LogWarning("User is authenticated but not found in database, attempting to create");
                    var createResult = await _userService.EnsureUserExistsAsync(principal);
                    if (!createResult.IsSuccess)
                    {
                        _logger.LogError("Failed to create user during state failure handling: {Error}", createResult.ErrorMessage);
                    }
                }

                return OAuthLoginResult.Success(
                    $"{_configuration["App:ClientUrl"]}?auth=success&note=authenticated_state_error",
                    null!);
            }

            _logger.LogWarning("User not authenticated and state validation failed");
            return OAuthLoginResult.Failure(
                $"{_configuration["App:ClientUrl"]}?error=authentication_failed&provider={provider}&details={Uri.EscapeDataString(error)}",
                "Authentication failed",
                error);
        }

        public async Task<OAuthLoginResult> HandleStateErrorAsync(ClaimsPrincipal principal, string provider, string error, string clientUrl)
        {
            _logger.LogWarning("State/correlation error in {Provider}Callback: {Message}", provider, error);

            if (principal.Identity?.IsAuthenticated == true)
            {
                _logger.LogInformation("User is already authenticated despite state error, redirecting to client");
                return await Task.FromResult(OAuthLoginResult.Success($"{clientUrl}?auth=success&note=authenticated_state_error", null!));
            }

            return await Task.FromResult(OAuthLoginResult.Failure(
                $"{clientUrl}?error=authentication_failed&provider={provider}&details={Uri.EscapeDataString(error)}",
                "Authentication failed",
                error));
        }

        private async Task<OAuthLoginResult> ProcessExternalLoginAsync(ExternalLoginInfo info, string clientUrl)
        {
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("External login failed: no email claim found");
                return OAuthLoginResult.Failure(
                    $"{clientUrl}?error=authentication_failed&message=no_email",
                    "Authentication failed",
                    "No email address found in the login information");
            }

            _logger.LogInformation("Processing external login for email: {Email} with provider: {Provider}",
                email, info.LoginProvider);

            // First, try to find a user with this specific external login
            var userWithLogin = await FindUserByExternalLoginAsync(info.LoginProvider, info.ProviderKey);
            if (userWithLogin != null)
            {
                _logger.LogInformation("User found with external login: {UserId}", userWithLogin.Id);
                await _signInManager.SignInAsync(userWithLogin, isPersistent: true);
                return OAuthLoginResult.Success(clientUrl, null!);
            }

            // If no user with this specific external login, check by email
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                _logger.LogInformation("User found by email: {UserId}", existingUser.Id);

                // Add this new login to the existing user
                _logger.LogInformation("Adding new login for provider: {Provider}, key: {Key}",
                    info.LoginProvider, info.ProviderKey);

                var res = await _userManager.AddLoginAsync(existingUser, info);
                if (!res.Succeeded)
                {
                    var errors = string.Join(", ", res.Errors.Select(e => e.Description));
                    _logger.LogWarning("Failed to add external login to existing user: {Errors}", errors);
                    return OAuthLoginResult.Failure(
                        $"{clientUrl}?error=login_failed&message={Uri.EscapeDataString(errors)}",
                        "Login failed",
                        errors);
                }

                // Update user details if appropriate
                await UpdateUserDetailsFromProvider(existingUser, info);

                // Sign in the user
                await _signInManager.SignInAsync(existingUser, isPersistent: true);
                return OAuthLoginResult.Success($"{clientUrl}?auth=success&provider={info.LoginProvider}", null!);
            }

            // Create new user if not exists
            _logger.LogInformation("Creating new user for email: {Email} with provider: {Provider}",
                email, info.LoginProvider);

            var (firstName, lastName) = _providerService.ExtractNameFromClaims(info.Principal, info.LoginProvider);

            var newUser = new AppUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true, // Auto-confirm for OAuth users
                FirstName = firstName,
                LastName = lastName
            };

            _logger.LogInformation("New user details - Email: {Email}, FirstName: {FirstName}, LastName: {LastName}",
                email, firstName, lastName);

            var createResult = await _userManager.CreateAsync(newUser);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                _logger.LogError("Failed to create user: {Errors}", errors);
                return OAuthLoginResult.Failure(
                    $"{clientUrl}?error=user_creation_failed&message={Uri.EscapeDataString(errors)}",
                    "User creation failed",
                    errors);
            }

            _logger.LogInformation("User created successfully with ID: {UserId}", newUser.Id);

            // Add user to default role
            var roleResult = await _userManager.AddToRoleAsync(newUser, "User");
            if (!roleResult.Succeeded)
            {
                _logger.LogWarning("Failed to add user to role: {Errors}",
                    string.Join(", ", roleResult.Errors.Select(e => e.Description)));
            }

            // Add the external login to the new user
            _logger.LogInformation("Adding external login - Provider: {Provider}, Key: {Key}",
                info.LoginProvider, info.ProviderKey);

            var addLoginResult = await _userManager.AddLoginAsync(newUser, info);
            if (!addLoginResult.Succeeded)
            {
                var errors = string.Join(", ", addLoginResult.Errors.Select(e => e.Description));
                _logger.LogError("Failed to add external login: {Errors}", errors);
                return OAuthLoginResult.Failure(
                    $"{clientUrl}?error=external_login_failed&message={Uri.EscapeDataString(errors)}",
                    "External login association failed",
                    errors);
            }

            _logger.LogInformation("External login added successfully");

            // Sign in the user
            await _signInManager.SignInAsync(newUser, isPersistent: true);
            _logger.LogInformation("User signed in: {UserId}", newUser.Id);

            return OAuthLoginResult.Success($"{clientUrl}?auth=success", null!);
        }

        private async Task<AppUser?> FindUserByExternalLoginAsync(string loginProvider, string providerKey)
        {
            _logger.LogInformation("Finding user by external login provider: {LoginProvider}, provider key: {ProviderKey}",
                loginProvider, providerKey);

            var users = _userManager.Users.ToList();
            foreach (var user in users)
            {
                var logins = await _userManager.GetLoginsAsync(user);

                if (logins.Any(l => l.LoginProvider == loginProvider && l.ProviderKey == providerKey))
                {
                    _logger.LogInformation("User found with external login: {UserId}", user.Id);
                    return user;
                }
            }

            _logger.LogWarning("No user found with external login provider: {LoginProvider}, provider key: {ProviderKey}",
                loginProvider, providerKey);

            return null;
        }

        private async Task UpdateUserDetailsFromProvider(AppUser user, ExternalLoginInfo info)
        {
            var (firstName, lastName) = _providerService.ExtractNameFromClaims(info.Principal, info.LoginProvider);
            bool userUpdated = false;

            // For GitHub users, if we're signing in with Microsoft and have more complete name info
            if (info.LoginProvider == "Microsoft" && !string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName))
            {
                // Update first name if it was previously a GitHub username
                if (string.IsNullOrEmpty(user.LastName) ||
                    (user.FirstName == user.Email || user.FirstName?.Contains('@') == true))
                {
                    user.FirstName = firstName;
                    userUpdated = true;
                }

                // Update last name if it was empty
                if (string.IsNullOrEmpty(user.LastName))
                {
                    user.LastName = lastName;
                    userUpdated = true;
                }
            }
            // For basic cases, only update if fields are empty
            else
            {
                if (string.IsNullOrEmpty(user.FirstName) && !string.IsNullOrEmpty(firstName))
                {
                    user.FirstName = firstName;
                    userUpdated = true;
                }

                if (string.IsNullOrEmpty(user.LastName) && !string.IsNullOrEmpty(lastName))
                {
                    user.LastName = lastName;
                    userUpdated = true;
                }
            }

            if (userUpdated)
            {
                _logger.LogInformation("Updating user details for {UserId} from external login", user.Id);
                await _userManager.UpdateAsync(user);
            }

            _logger.LogInformation("User details updated for {UserId}", user.Id);
        }
    }
}