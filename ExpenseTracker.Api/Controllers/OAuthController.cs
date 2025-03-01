using ExpenseTracker.Infrastructure.Identity.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ExpenseTracker.Api.Controllers
{
    [Route("api/oauth")]
    [ApiController]
    public class OAuthController(
        SignInManager<AppUser> signInManager,
        UserManager<AppUser> userManager,
        IConfiguration configuration,
        ILogger<OAuthController> logger) : ControllerBase
    {

        private readonly SignInManager<AppUser> _signInManager = signInManager;
        private readonly UserManager<AppUser> _userManager = userManager;
        private readonly IConfiguration _configuration = configuration;
        private readonly ILogger<OAuthController> _logger = logger;

        [HttpGet("login")]
        public IActionResult Login(string returnUrl = "/")
        {
            _logger.LogInformation("Login endpoint called with returnUrl: {ReturnUrl}", returnUrl);
            return returnUrl is null ? throw new ArgumentNullException(nameof(returnUrl))
                : (IActionResult)Ok(new { loginUrl = Url.Action("ExternalLogin") });
        }

        [HttpGet("login/{provider}")]
        public IActionResult ExternalLogin(string provider, string returnUrl = null!)
        {
            try
            {
                _logger.LogInformation("ExternalLogin endpoint called with provider: {Provider} and returnUrl: {ReturnUrl}", provider, returnUrl);
                returnUrl ??= _configuration["App:ClientUrl"]!;

                // Standardize the provider name first
                string normalizedProvider = provider.ToLower() switch
                {
                    "microsoft" => "Microsoft",
                    "github" => "GitHub",
                    _ => throw new ArgumentException($"Unsupported provider: {provider}")
                };

                var properties = new AuthenticationProperties
                {
                    RedirectUri = Url.Action(
                        action: $"{normalizedProvider}Callback",
                        controller: "OAuth",
                        values: null,
                        protocol: Request.Scheme),
                    AllowRefresh = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10),
                    IsPersistent = true
                };

                properties.Items["returnUrl"] = returnUrl;

                _logger.LogInformation("Initiating challenge for provider: {Provider}", normalizedProvider);
                return Challenge(properties, normalizedProvider);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Authentication challenge failed for provider: {Provider}", provider);
                return StatusCode(500, new { message = "Authentication challenge failed: " + ex.Message });
            }
        }

        [HttpGet("microsoft-callback")]
        public async Task<IActionResult> MicrosoftCallback()
        {
            _logger.LogInformation("MicrosoftCallback endpoint called.");

            try
            {
                // Check if this is a direct request without code/state parameters
                if (!Request.Query.ContainsKey("code") && !Request.Query.ContainsKey("state"))
                {
                    // If user is already authenticated, just redirect to client
                    if (User.Identity?.IsAuthenticated == true)
                    {
                        _logger.LogInformation("User is already authenticated, redirecting to client");
                        return Redirect(_configuration["App:ClientUrl"]!);
                    }

                    _logger.LogWarning("MicrosoftCallback called without code or state parameters.");
                    return Redirect($"{_configuration["App:ClientUrl"]}?error=no_params&provider=Microsoft");
                }

                var info = await _signInManager.GetExternalLoginInfoAsync();

                if (info == null)
                {
                    _logger.LogWarning("No external login info found for Microsoft.");

                    // Check if the user is already authenticated despite missing external login info
                    if (User.Identity?.IsAuthenticated == true)
                    {
                        _logger.LogInformation("User is authenticated but no external login info found, redirecting to client");
                        return Redirect($"{_configuration["App:ClientUrl"]}?auth=success&note=already_authenticated");
                    }

                    return Redirect($"{_configuration["App:ClientUrl"]}?error=authentication_failed&provider=Microsoft&message=no_login_info");
                }

                _logger.LogInformation("External login info retrieved successfully.");
                return await ProcessExternalLogin(info);
            }
            catch (Exception ex) when (ex.Message.Contains("state") || ex.Message.Contains("correlation"))
            {
                _logger.LogWarning("State/correlation error in MicrosoftCallback: {Message}", ex.Message);

                // Check if the user is already authenticated despite the state error
                if (User.Identity?.IsAuthenticated == true)
                {
                    _logger.LogInformation("User is already authenticated despite state error, redirecting to client");
                    return Redirect($"{_configuration["App:ClientUrl"]}?auth=success&note=authenticated_state_error");
                }

                // Redirect to the client with a specific error message
                return Redirect($"{_configuration["App:ClientUrl"]}?error=state_error&message={Uri.EscapeDataString(ex.Message)}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in MicrosoftCallback");
                return Redirect($"{_configuration["App:ClientUrl"]}?error=unknown&message={Uri.EscapeDataString(ex.Message)}");
            }
        }

        [HttpGet("github-callback")]
        public async Task<IActionResult> GitHubCallback()
        {
            _logger.LogInformation("GitHubCallback endpoint called.");

            try
            {
                // Check if this is a direct request without code/state parameters
                if (!Request.Query.ContainsKey("code") && !Request.Query.ContainsKey("state"))
                {
                    // If user is already authenticated, just redirect to client
                    if (User.Identity?.IsAuthenticated == true)
                    {
                        _logger.LogInformation("User is already authenticated, redirecting to client");
                        return Redirect(_configuration["App:ClientUrl"]!);
                    }

                    _logger.LogWarning("GitHubCallback called without code or state parameters.");
                    return Redirect($"{_configuration["App:ClientUrl"]}?error=no_params&provider=GitHub");
                }

                var info = await _signInManager.GetExternalLoginInfoAsync();

                if (info == null)
                {
                    _logger.LogWarning("No external login info found for GitHub.");

                    // Check if the user is already authenticated despite missing external login info
                    if (User.Identity?.IsAuthenticated == true)
                    {
                        _logger.LogInformation("User is authenticated but no external login info found, redirecting to client");
                        return Redirect($"{_configuration["App:ClientUrl"]}?auth=success&note=already_authenticated");
                    }

                    return Redirect($"{_configuration["App:ClientUrl"]}?error=authentication_failed&provider=GitHub&message=no_login_info");
                }

                _logger.LogInformation("External login info retrieved successfully.");
                return await ProcessExternalLogin(info);
            }
            catch (Exception ex) when (ex.Message.Contains("state") || ex.Message.Contains("correlation"))
            {
                _logger.LogWarning("State/correlation error in GitHubCallback: {Message}", ex.Message);

                // Check if the user is already authenticated despite the state error
                if (User.Identity?.IsAuthenticated == true)
                {
                    _logger.LogInformation("User is already authenticated despite state error, redirecting to client");
                    return Redirect($"{_configuration["App:ClientUrl"]}?auth=success&note=authenticated_state_error");
                }

                // Redirect to the client with a specific error message
                return Redirect($"{_configuration["App:ClientUrl"]}?error=state_error&message={Uri.EscapeDataString(ex.Message)}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in GitHubCallback");
                return Redirect($"{_configuration["App:ClientUrl"]}?error=unknown&message={Uri.EscapeDataString(ex.Message)}");
            }
        }

        [HttpGet("handle-state-failure")]
        public IActionResult HandleStateFailure(string provider, string error)
        {
            _logger.LogInformation($"Handling state validation failure for {provider}");

            // Check if the user is authenticated despite the state error
            if (User.Identity?.IsAuthenticated == true)
            {
                _logger.LogInformation("User is already authenticated despite state error, redirecting to success");
                return Redirect($"{_configuration["App:ClientUrl"]}/dashboard?auth=success");
            }

            _logger.LogError($"Request Query: {Request.QueryString}");
            _logger.LogError($"Cookies: {string.Join(", ", Request.Cookies.Select(c => $"{c.Key}={c.Value}"))}");

            // Only redirect with error if not authenticated
            return Redirect($"{_configuration["App:ClientUrl"]}?error=authentication_failed&provider={provider}&details={Uri.EscapeDataString(error)}");
        }

        [HttpGet("status")]
        public async Task<IActionResult> CheckAuthStatus()
        {
            try
            {
                if (!User.Identity!.IsAuthenticated)
                {
                    return Ok(new { success = false, message = "User not authenticated" });
                }

                _logger.LogInformation("Auth status check for authenticated user");

                // Log all claims for debugging
                foreach (var claim in User.Claims)
                {
                    _logger.LogInformation("Claim: {Type} = {Value}", claim.Type, claim.Value);
                }

                // Determine correct provider 
                string providerName;
                if (User.HasClaim(c => c.Type == "urn:github:login") ||
                    User.HasClaim(c => c.Type == "urn:github:name") ||
                    User.FindFirstValue(ClaimTypes.NameIdentifier)?.Contains("github", StringComparison.OrdinalIgnoreCase) == true)
                {
                    providerName = "GitHub";
                }
                else if (User.HasClaim(c => c.Type == "http://schemas.microsoft.com/identity/claims/identityprovider"))
                {
                    providerName = "Microsoft";
                }
                else
                {
                    // Default to Microsoft if can't determine
                    providerName = "Microsoft";
                }

                _logger.LogInformation("Determined provider: {Provider}", providerName);

                // More robust user lookup
                var nameIdentifier = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var email = User.FindFirstValue(ClaimTypes.Email);
                var name = User.FindFirstValue(ClaimTypes.Name);

                _logger.LogInformation("Looking up user - NameIdentifier: {NameIdentifier}, Email: {Email}, Name: {Name}",
                    nameIdentifier, email, name);

                if (string.IsNullOrEmpty(nameIdentifier) && string.IsNullOrEmpty(email))
                {
                    return Ok(new { success = false, error = "No valid identifier found in claims" });
                }

                // Try to find user by external login first
                AppUser user = null!;
                if (!string.IsNullOrEmpty(nameIdentifier))
                {
                    user = await FindUserByExternalLogin(nameIdentifier);
                    if (user != null)
                    {
                        _logger.LogInformation("User found by external login: {UserId}", user.Id);

                        // Ensure this specific provider is linked
                        await EnsureProviderLinked(user, nameIdentifier, providerName);
                    }
                }

                // If not found, try by email
                if (user == null && !string.IsNullOrEmpty(email))
                {
                    user = await _userManager.FindByEmailAsync(email)!;
                    if (user != null)
                    {
                        _logger.LogInformation("User found by email: {UserId}", user.Id);

                        // Add the external login if we have a nameIdentifier
                        if (!string.IsNullOrEmpty(nameIdentifier))
                        {
                            await EnsureProviderLinked(user, nameIdentifier, providerName);
                        }
                    }
                }

                // If still no user found, create one
                if (user == null)
                {
                    _logger.LogInformation("No existing user found, creating new user");
                    user = await CreateNewUser(providerName);

                    if (user != null)
                    {
                        _logger.LogInformation("New user created: {UserId}", user.Id);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to create new user");
                        return Ok(new { success = false, error = "Failed to create new user" });
                    }
                }

                var roles = await _userManager.GetRolesAsync(user);

                var userResponse = new
                {
                    id = user.Id.ToString(),
                    email = user.Email,
                    displayName = !string.IsNullOrEmpty(user.UserName) ? user.UserName : user.Email,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    roles = roles.ToArray()
                };

                return Ok(new
                {
                    success = true,
                    user = userResponse
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CheckAuthStatus");
                return Ok(new { success = false, error = ex.Message });
            }
        }

        [HttpGet("ensure-user")]
        public async Task<IActionResult> EnsureUserExists()
        {
            if (!User.Identity!.IsAuthenticated)
            {
                return Ok(new { success = false, message = "Not authenticated" });
            }

            _logger.LogInformation("EnsureUserExists called for authenticated user");

            // Log claims for debugging
            foreach (var claim in User.Claims)
            {
                _logger.LogInformation("Claim: {Type} = {Value}", claim.Type, claim.Value);
            }

            var email = User.FindFirstValue(ClaimTypes.Email);
            var nameIdentifier = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("No email claim found");
                return Ok(new { success = false, error = "No email claim found" });
            }

            // Determine provider - same logic as in CreateNewUser
            string provider;
            if (User.HasClaim(c => c.Type == "http://schemas.microsoft.com/identity/claims/identityprovider"))
            {
                provider = "Microsoft";
            }
            else if (User.HasClaim(c => c.Type == "urn:github:login"))
            {
                provider = "GitHub";
            }
            else if (User.FindFirstValue(ClaimTypes.NameIdentifier)?.Contains("github") == true)
            {
                provider = "GitHub";
            }
            else
            {
                provider = HttpContext.Request.Path.Value?.Contains("github", StringComparison.OrdinalIgnoreCase) == true
                    ? "GitHub"
                    : "Microsoft";
            }

            // Try to find user by external login first
            AppUser user = null!;

            if (!string.IsNullOrEmpty(nameIdentifier))
            {
                user = await FindUserByExternalLogin(nameIdentifier);
                if (user != null)
                {
                    _logger.LogInformation("User found by external login: {UserId}", user.Id);

                    // Check if this specific provider is already connected
                    var logins = await _userManager.GetLoginsAsync(user);
                    if (!logins.Any(l => l.LoginProvider == provider && l.ProviderKey == nameIdentifier))
                    {
                        // Add this provider login
                        _logger.LogInformation("Adding additional provider login: {Provider}", provider);
                        var displayName = User.FindFirstValue(ClaimTypes.Name) ?? email;
                        var loginInfo = new UserLoginInfo(provider, nameIdentifier, displayName);
                        await _userManager.AddLoginAsync(user, loginInfo);
                    }
                }
            }

            // If still no user found, create one
            if (user == null)
            {
                _logger.LogInformation("Creating new user from authenticated claims");
                user = await CreateNewUser(provider);

                if (user == null)
                {
                    _logger.LogWarning("Failed to create user");
                    return Ok(new { success = false, error = "Failed to create user" });
                }
            }

            // Ensure we have a fresh sign-in
            await _signInManager.RefreshSignInAsync(user);

            // Return user data
            var roles = await _userManager.GetRolesAsync(user);

            var userResponse = new
            {
                id = user.Id.ToString(),
                email = user.Email,
                displayName = !string.IsNullOrEmpty(user.UserName) ? user.UserName : user.Email,
                firstName = user.FirstName,
                lastName = user.LastName,
                roles = roles.ToArray()
            };

            return Ok(new
            {
                success = true,
                user = userResponse
            });
        }

        private async Task<IActionResult> ProcessExternalLogin(ExternalLoginInfo info)
        {
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("External login failed: no email claim found.");
                return Redirect($"{_configuration["App:ClientUrl"]}?error=authentication_failed&message=no_email");
            }

            _logger.LogInformation("Processing external login for email: {Email} with provider: {Provider}",
                email, info.LoginProvider);

            // First, try to find a user with this specific external login
            var userWithLogin = await FindUserByExternalLogin(info.LoginProvider, info.ProviderKey);
            if (userWithLogin != null)
            {
                _logger.LogInformation("User found with external login: {UserId}", userWithLogin.Id);
                await _signInManager.SignInAsync(userWithLogin, isPersistent: true);
                return Redirect(_configuration["App:ClientUrl"]!);
            }

            // If no user with this specific external login, check by email
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                _logger.LogInformation("User found by email: {UserId}", existingUser.Id);

                // Check existing logins for this user
                var existingLogins = await _userManager.GetLoginsAsync(existingUser);

                // Log the existing logins
                foreach (var login in existingLogins)
                {
                    _logger.LogInformation("Existing login: Provider={Provider}, Key={Key}",
                        login.LoginProvider, login.ProviderKey);
                }

                // Add this new login to the existing user
                _logger.LogInformation("Adding new login for provider: {Provider}", info.LoginProvider);
                var addLoginResult = await _userManager.AddLoginAsync(existingUser, info);
                if (!addLoginResult.Succeeded)
                {
                    var errors = string.Join(", ", addLoginResult.Errors.Select(e => e.Description));
                    _logger.LogWarning("Failed to add external login to existing user: {Errors}", errors);
                    return Redirect($"{_configuration["App:ClientUrl"]}?error=login_failed&message={Uri.EscapeDataString(errors)}");
                }

                // Update user details if they are empty or if we have better info from this provider
                bool userUpdated = false;

                // Extract names from claims based on provider
                var (firstName, lastName) = ExtractNameFromClaims(info.Principal, info.LoginProvider);

                // For GitHub users, if we're signing in with Microsoft and have more complete name info
                if (info.LoginProvider == "Microsoft" && !string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName))
                {
                    // Update first name if it was previously a GitHub username
                    if (string.IsNullOrEmpty(existingUser.LastName) ||
                        (existingUser.FirstName == existingUser.Email || existingUser.FirstName?.Contains('@') == true))
                    {
                        existingUser.FirstName = firstName;
                        userUpdated = true;
                    }

                    // Update last name if it was empty
                    if (string.IsNullOrEmpty(existingUser.LastName))
                    {
                        existingUser.LastName = lastName;
                        userUpdated = true;
                    }
                }
                // For basic cases, only update if fields are empty
                else
                {
                    if (string.IsNullOrEmpty(existingUser.FirstName) && !string.IsNullOrEmpty(firstName))
                    {
                        existingUser.FirstName = firstName;
                        userUpdated = true;
                    }

                    if (string.IsNullOrEmpty(existingUser.LastName) && !string.IsNullOrEmpty(lastName))
                    {
                        existingUser.LastName = lastName;
                        userUpdated = true;
                    }
                }

                if (userUpdated)
                {
                    var updateResult = await _userManager.UpdateAsync(existingUser);
                    if (updateResult.Succeeded)
                    {
                        _logger.LogInformation("User details updated for user: {UserId}", existingUser.Id);
                    }
                    else
                    {
                        var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                        _logger.LogWarning("Failed to update user details: {Errors}", errors);
                    }
                }

                await _signInManager.SignInAsync(existingUser, isPersistent: true);
                return Redirect($"{_configuration["App:ClientUrl"]}?auth=success&provider={info.LoginProvider}");
            }

            // Create new user if not exists
            var (first, last) = ExtractNameFromClaims(info.Principal, info.LoginProvider);

            var user = new AppUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName = first,
                LastName = last
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                _logger.LogWarning("Failed to create new user: {Errors}", errors);
                return Redirect($"{_configuration["App:ClientUrl"]}?error=user_creation_failed&message={Uri.EscapeDataString(errors)}");
            }

            _logger.LogInformation("New user created: {UserId}", user.Id);

            // Add user to the "User" role
            var roleResult = await _userManager.AddToRoleAsync(user, "User");
            if (!roleResult.Succeeded)
            {
                var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                _logger.LogWarning("Failed to add user to role: {Errors}", errors);
            }

            // Add external login - make sure to use the correct provider from info
            _logger.LogInformation("Adding external login with provider: {Provider}", info.LoginProvider);
            var addExternalResult = await _userManager.AddLoginAsync(user, info);
            if (!addExternalResult.Succeeded)
            {
                var errors = string.Join(", ", addExternalResult.Errors.Select(e => e.Description));
                _logger.LogWarning("Failed to add external login: {Errors}", errors);
                return Redirect($"{_configuration["App:ClientUrl"]}?error=external_login_failed&message={Uri.EscapeDataString(errors)}");
            }

            await _signInManager.SignInAsync(user, isPersistent: true);
            _logger.LogInformation("User signed in: {UserId}", user.Id);
            return Redirect($"{_configuration["App:ClientUrl"]}?auth=success");
        }

        private async Task<AppUser> FindUserByExternalLogin(string loginProvider, string providerKey)
        {
            _logger.LogInformation("Finding user by external login provider: {LoginProvider}, provider key: {ProviderKey}", loginProvider, providerKey);
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
            _logger.LogWarning("No user found with external login provider: {LoginProvider}, provider key: {ProviderKey}", loginProvider, providerKey);
            return null!;
        }

        private async Task<AppUser> FindUserByExternalLogin(string nameIdentifier)
        {
            _logger.LogInformation("Finding user by external login name identifier: {NameIdentifier}", nameIdentifier);
            if (string.IsNullOrEmpty(nameIdentifier))
            {
                _logger.LogWarning("Name identifier is null or empty.");
                return null!;
            }

            var users = _userManager.Users.ToList();
            foreach (var user in users)
            {
                var logins = await _userManager.GetLoginsAsync(user);
                if (logins.Any(l => l.ProviderKey == nameIdentifier))
                {
                    _logger.LogInformation("User found with external login: {UserId}", user.Id);
                    return user;
                }
            }
            _logger.LogWarning("No user found with external login name identifier: {NameIdentifier}", nameIdentifier);
            return null!;
        }

        private async Task<AppUser> CreateNewUser(string providerName)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("Cannot create user without email claim");
                return null!;
            }

            var nameIdentifier = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _logger.LogInformation("Creating new user with provider: {Provider}", providerName);

            // Check if user already exists by email to avoid duplicates
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                _logger.LogInformation("User with email {Email} already exists", email);

                // Add the external login to the existing user if needed
                await EnsureProviderLinked(existingUser, nameIdentifier!, providerName);

                // Refresh the sign-in
                await _signInManager.RefreshSignInAsync(existingUser);
                return existingUser;
            }

            // Extract name information based on provider
            var (firstName, lastName) = ExtractNameFromClaims(User, providerName);

            // Create new user
            var user = new AppUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true, // Auto-confirm for OAuth users
                FirstName = firstName,
                LastName = lastName
            };

            _logger.LogInformation("Creating new user with UserName={UserName}, Email={Email}",
                                  user.UserName, user.Email);

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                _logger.LogError("Failed to create user: {Errors}", errors);
                return null!;
            }

            // Add the user to the User role
            var roleResult = await _userManager.AddToRoleAsync(user, "User");
            if (!roleResult.Succeeded)
            {
                var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                _logger.LogWarning("Failed to add user to User role: {Errors}", errors);
            }

            // Add the external login
            await EnsureProviderLinked(user, nameIdentifier!, providerName);

            // Sign in the user
            await _signInManager.SignInAsync(user, isPersistent: true);

            return user;
        }

        private (string firstName, string lastName) ExtractNameFromClaims(ClaimsPrincipal principal, string provider)
        {
            string firstName = "", lastName = "";

            if (provider == "Microsoft")
            {
                // Microsoft provides separate GivenName and Surname claims
                firstName = principal.FindFirstValue(ClaimTypes.GivenName) ?? "";
                lastName = principal.FindFirstValue(ClaimTypes.Surname) ?? "";
            }
            else if (provider == "GitHub")
            {
                // For GitHub, prioritize the full name (urn:github:name) over the username (ClaimTypes.Name)
                var fullName = principal.FindFirstValue("urn:github:name") ??
                              principal.FindFirstValue(ClaimTypes.Name) ?? "";

                _logger.LogInformation("GitHub full name: {FullName}", fullName);

                if (!string.IsNullOrEmpty(fullName) && fullName.Contains(" "))
                {
                    // Split full name into first and last name
                    var parts = fullName.Split(' ', 2);
                    firstName = parts[0];
                    lastName = parts.Length > 1 ? parts[1] : "";
                }
                else
                {
                    // If no space in name, only use the full name as first name
                    firstName = fullName;
                    lastName = "";
                }
            }
            else
            {
                // Generic fallback for any provider
                firstName = principal.FindFirstValue(ClaimTypes.GivenName) ??
                           principal.FindFirstValue(ClaimTypes.Name)?.Split(' ').FirstOrDefault() ?? "";

                lastName = principal.FindFirstValue(ClaimTypes.Surname) ??
                          (principal.FindFirstValue(ClaimTypes.Name)?.Split(' ').Skip(1).FirstOrDefault() ?? "");
            }

            _logger.LogInformation("Extracted name from claims - Provider: {Provider}, FirstName: {FirstName}, LastName: {LastName}",
                                 provider, firstName, lastName);

            return (firstName, lastName);
        }

        private async Task EnsureProviderLinked(AppUser user, string nameIdentifier, string providerName)
        {
            // Get existing logins
            var logins = await _userManager.GetLoginsAsync(user);

            // Check if this specific provider login already exists
            if (!logins.Any(l => l.LoginProvider == providerName && l.ProviderKey == nameIdentifier))
            {
                _logger.LogInformation("Adding {Provider} login to user {UserId}", providerName, user.Id);

                // Create display name based on available claims
                string displayName;
                if (providerName == "GitHub")
                {
                    displayName = User.FindFirstValue("urn:github:name") ??
                                 User.FindFirstValue(ClaimTypes.Name) ??
                                 user.Email!;
                }
                else
                {
                    displayName = User.FindFirstValue(ClaimTypes.Name) ??
                                 $"{user.FirstName} {user.LastName}".Trim() ??
                                 user.Email!;
                }

                // Add the login
                var loginInfo = new UserLoginInfo(providerName, nameIdentifier, displayName);
                var result = await _userManager.AddLoginAsync(user, loginInfo);

                if (!result.Succeeded)
                {
                    _logger.LogWarning("Failed to add {Provider} login: {Errors}",
                        providerName,
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
                else
                {
                    _logger.LogInformation("Successfully added {Provider} login to user {UserId}", providerName, user.Id);
                }
            }
            else
            {
                _logger.LogInformation("{Provider} login already exists for user {UserId}", providerName, user.Id);
            }
        }
    }
}