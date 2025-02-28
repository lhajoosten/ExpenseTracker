using ExpenseTracker.Infrastructure.Identity.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ExpenseTracker.Api.Controllers
{
    [Route("api/oauth")]
    [ApiController]
    public class OAuthController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, IConfiguration configuration) : ControllerBase
    {

        private readonly SignInManager<AppUser> _signInManager = signInManager;
        private readonly UserManager<AppUser> _userManager = userManager;
        private readonly IConfiguration _configuration = configuration;

        [HttpGet("login")]
        public IActionResult Login(string returnUrl = "/")
        {
            return Ok(new { loginUrl = Url.Action("ExternalLogin") });
        }

        [HttpGet("login/{provider}")]
        public IActionResult ExternalLogin(string provider, string returnUrl = null!)
        {
            try
            {
                Console.WriteLine($"Provider requested: {provider}");

                returnUrl ??= _configuration["App:ClientUrl"]!;

                // Standardize the provider name first
                string normalizedProvider = provider.ToLower() switch
                {
                    "microsoft" => "Microsoft",
                    "github" => "GitHub",
                    _ => throw new ArgumentException($"Unsupported provider: {provider}")
                };

                // Use the normalized provider name for everything
                Console.WriteLine($"Using authentication scheme: {normalizedProvider}");

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

                Console.WriteLine($"Starting authentication challenge for {normalizedProvider}");

                return Challenge(properties, normalizedProvider);
            }
            catch (Exception ex)
            {
                // Your existing error handling
                Console.WriteLine($"Error in ExternalLogin: {ex}");
                return StatusCode(500, new { message = "Authentication challenge failed" });
            }
        }

        [HttpGet("microsoft-callback")]
        public async Task<IActionResult> MicrosoftCallback()
        {
            // Check if this is a direct request without code/state parameters
            if (!Request.Query.ContainsKey("code") && !Request.Query.ContainsKey("state"))
            {
                return Redirect($"{_configuration["App:ClientUrl"]}?error=no_params&provider=Microsoft");
            }

            try
            {
                var info = await _signInManager.GetExternalLoginInfoAsync();

                if (info == null)
                {
                    return Redirect($"{_configuration["App:ClientUrl"]}?error=authentication_failed&provider=Microsoft&message=no_login_info");
                }

                Console.WriteLine($"Microsoft login info successfully retrieved: {info.LoginProvider}");
                return await ProcessExternalLogin(info);
            }
            catch (Exception ex) when (ex.Message.Contains("state") || ex.Message.Contains("correlation"))
            {
                // Check if user is already authenticated
                if (User.Identity.IsAuthenticated)
                {
                    // Already logged in, just redirect to client
                    return Redirect(_configuration["App:ClientUrl"]);
                }

                // Otherwise redirect with error
                return Redirect($"{_configuration["App:ClientUrl"]}?error=state_error&message={Uri.EscapeDataString(ex.Message)}");
            }
        }

        [HttpGet("github-callback")]
        public async Task<IActionResult> GitHubCallback()
        {
            // Check if this is a direct request without code/state parameters
            if (!Request.Query.ContainsKey("code") && !Request.Query.ContainsKey("state"))
            {
                return Redirect($"{_configuration["App:ClientUrl"]}?error=no_params&provider=GitHub");
            }

            try
            {
                var info = await _signInManager.GetExternalLoginInfoAsync();

                if (info == null)
                {
                    return Redirect($"{_configuration["App:ClientUrl"]}?error=authentication_failed&provider=GitHub&message=no_login_info");
                }

                Console.WriteLine($"GitHub login info successfully retrieved: {info.LoginProvider}");
                return await ProcessExternalLogin(info);
            }
            catch (Exception ex) when (ex.Message.Contains("state") || ex.Message.Contains("correlation"))
            {
                // Check if user is already authenticated
                if (User.Identity.IsAuthenticated)
                {
                    // Already logged in, just redirect to client
                    return Redirect(_configuration["App:ClientUrl"]);
                }

                // Otherwise redirect with error
                return Redirect($"{_configuration["App:ClientUrl"]}?error=state_error&message={Uri.EscapeDataString(ex.Message)}");
            }
        }

        [HttpGet("handle-state-failure")]
        public IActionResult HandleStateFailure(string provider, string error)
        {
            Console.WriteLine($"Handling state validation failure for {provider}");
            Console.WriteLine($"Error details: {error}");

            // Log additional context
            Console.WriteLine($"Request Query: {Request.QueryString}");
            Console.WriteLine($"Cookies: {string.Join(", ", Request.Cookies.Select(c => $"{c.Key}={c.Value}"))}");

            // Redirect to client app with detailed error
            return Redirect($"{_configuration["App:ClientUrl"]}?error=authentication_failed&provider={provider}&details={Uri.EscapeDataString(error)}");
        }

        private async Task<IActionResult> ProcessExternalLogin(ExternalLoginInfo info)
        {
            // Log all claims with their full details
            Console.WriteLine("Detailed Claims:");
            foreach (var claim in info.Principal.Claims)
            {
                Console.WriteLine($"Type: {claim.Type}, Value: {claim.Value}, Issuer: {claim.Issuer}");
            }

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
            {
                Console.WriteLine("No email found in external login info");
                return Redirect($"{_configuration["App:ClientUrl"]}?error=authentication_failed&message=no_email");
            }

            Console.WriteLine($"Processing external login for email: {email}, Provider: {info.LoginProvider}");

            // First, try to find a user with this specific external login
            var userWithLogin = await FindUserByExternalLogin(info.LoginProvider, info.ProviderKey);
            if (userWithLogin != null)
            {
                await _signInManager.SignInAsync(userWithLogin, isPersistent: true);
                Console.WriteLine($"Signed in user with existing {info.LoginProvider} login");
                return Redirect(_configuration["App:ClientUrl"]!);
            }

            // If no user with this specific external login, check by email
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                // Check existing logins for this user
                var existingLogins = await _userManager.GetLoginsAsync(existingUser);

                Console.WriteLine("Existing Logins:");
                foreach (var login in existingLogins)
                {
                    Console.WriteLine($"Provider: {login.LoginProvider}, Key: {login.ProviderKey}");
                }

                // Add this new login to the existing user
                var addLoginResult = await _userManager.AddLoginAsync(existingUser, info);
                if (!addLoginResult.Succeeded)
                {
                    var errors = string.Join(", ", addLoginResult.Errors.Select(e => e.Description));
                    Console.WriteLine($"Failed to add external login to existing user: {errors}");
                    return Redirect($"{_configuration["App:ClientUrl"]}?error=login_failed&message={Uri.EscapeDataString(errors)}");
                }

                // Update user details if they are empty
                bool userUpdated = false;

                // Extract names from claims
                var firstName = info.Principal.FindFirstValue(ClaimTypes.GivenName);
                var lastName = info.Principal.FindFirstValue(ClaimTypes.Surname);

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

                if (userUpdated)
                {
                    var updateResult = await _userManager.UpdateAsync(existingUser);
                    Console.WriteLine($"User update result: {updateResult.Succeeded}");
                    if (!updateResult.Succeeded)
                    {
                        Console.WriteLine($"Update errors: {string.Join(", ", updateResult.Errors.Select(e => e.Description))}");
                    }
                }

                await _signInManager.SignInAsync(existingUser, isPersistent: true);
                Console.WriteLine($"Added {info.LoginProvider} login to existing user");
                return Redirect($"{_configuration["App:ClientUrl"]}?auth=success&provider={info.LoginProvider}");
            }

            // Create new user if not exists
            var user = new AppUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName = info.Principal.FindFirstValue(ClaimTypes.GivenName) ?? "",
                LastName = info.Principal.FindFirstValue(ClaimTypes.Surname) ?? ""
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                Console.WriteLine($"Failed to create user: {errors}");
                return Redirect($"{_configuration["App:ClientUrl"]}?error=user_creation_failed&message={Uri.EscapeDataString(errors)}");
            }

            // Add external login
            var addExternalResult = await _userManager.AddLoginAsync(user, info);
            if (!addExternalResult.Succeeded)
            {
                var errors = string.Join(", ", addExternalResult.Errors.Select(e => e.Description));
                Console.WriteLine($"Failed to add external login: {errors}");
                return Redirect($"{_configuration["App:ClientUrl"]}?error=external_login_failed&message={Uri.EscapeDataString(errors)}");
            }

            await _signInManager.SignInAsync(user, isPersistent: true);
            Console.WriteLine($"Created new user with {info.LoginProvider} login");
            return Redirect($"{_configuration["App:ClientUrl"]}?auth=success");
        }

        private async Task<AppUser> FindUserByExternalLogin(string loginProvider, string providerKey)
        {
            Console.WriteLine($"Finding user by external login - Provider: {loginProvider}, Key: {providerKey}");

            var users = _userManager.Users.ToList();
            foreach (var user in users)
            {
                var logins = await _userManager.GetLoginsAsync(user);
                foreach (var login in logins)
                {
                    Console.WriteLine($"Checking user {user.Id} - Existing login: {login.LoginProvider}, {login.ProviderKey}");
                }

                if (logins.Any(l => l.LoginProvider == loginProvider && l.ProviderKey == providerKey))
                {
                    Console.WriteLine($"Found user {user.Id} with matching login");
                    return user;
                }
            }

            Console.WriteLine("No user found with matching external login");
            return null;
        }

        [HttpGet("callback-test")]
        public IActionResult CallbackTest()
        {
            return Content($"User is authenticated: {User.Identity.IsAuthenticated}. Cookies: {string.Join(", ", Request.Cookies.Keys)}");
        }
    }
}