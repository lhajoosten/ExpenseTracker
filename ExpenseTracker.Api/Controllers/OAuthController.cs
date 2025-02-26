using ExpenseTracker.Infrastructure.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ExpenseTracker.Api.Controllers
{
    [Route("api/oauth")]
    [ApiController]
    public class OAuthController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager) : ControllerBase
    {

        private readonly SignInManager<AppUser> _signInManager = signInManager;
        private readonly UserManager<AppUser> _userManager = userManager;

        [HttpGet("login")]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            // Build the redirect URL for the external provider callback
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet("login-callback")]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            // Handle any error from the external provider
            if (!string.IsNullOrEmpty(remoteError))
            {
                return RedirectToAction("Login", new { ReturnUrl = returnUrl, ErrorMessage = remoteError });
            }

            // Retrieve external login information
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction("Login", new { ReturnUrl = returnUrl, ErrorMessage = "Error loading external login information." });
            }

            // Attempt to sign in the user with the external login provider
            var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (signInResult.Succeeded)
            {
                return LocalRedirect(returnUrl ?? "/");
            }
            else
            {
                // Extract the email claim from the external provider
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                if (string.IsNullOrEmpty(email))
                {
                    return RedirectToAction("Login", new { ReturnUrl = returnUrl, ErrorMessage = "Email claim not received from external provider." });
                }

                // Create a new user using the email as the username
                var user = new AppUser { UserName = email, Email = email };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    return RedirectToAction("Login", new { ReturnUrl = returnUrl, ErrorMessage = errors });
                }

                // Associate the external login with the newly created user
                var addLoginResult = await _userManager.AddLoginAsync(user, info);
                if (!addLoginResult.Succeeded)
                {
                    var errors = string.Join(", ", addLoginResult.Errors.Select(e => e.Description));
                    // Optionally remove the created user if the login association fails
                    await _userManager.DeleteAsync(user);
                    return RedirectToAction("Login", new { ReturnUrl = returnUrl, ErrorMessage = errors });
                }

                // Sign in the user and redirect to the return URL
                await _signInManager.SignInAsync(user, isPersistent: false);
                return LocalRedirect(returnUrl ?? "/");
            }
        }
    }
}
