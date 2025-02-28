using ExpenseTracker.Common.Abstractions;
using ExpenseTracker.Common.Models;
using ExpenseTracker.Infrastructure.Identity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ExpenseTracker.Api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class IdentityController(IAuthService authService, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager) : ControllerBase
    {
        private readonly IAuthService _authService = authService;
        private readonly UserManager<AppUser> _userManager = userManager;
        private readonly SignInManager<AppUser> _signInManager = signInManager;

        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] SignUpRequest request)
        {
            var result = await _authService.SignUpAsync(request);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Errors);
            }
            return Ok();
        }

        [HttpPost("signin")]
        public async Task<IActionResult> SignIn([FromBody] SignInRequest request)
        {
            var result = await _authService.SignInAsync(request);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Errors);
            }
            return Ok();
        }

        [Authorize]
        [HttpGet("current")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var result = await _authService.GetCurrentUserAsync(User);
            if (!result.IsSuccess)
            {
                return NotFound(result.Errors);
            }
            return Ok(result.Value);
        }

        [Authorize]
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshSignIn()
        {
            await _authService.RefreshSignInAsync(User);
            return Ok();
        }

        [Authorize]
        [HttpPost("signout")]
        public async Task<IActionResult> SignOutFromSession()
        {
            await _authService.SignOutAsync();
            return Ok();
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var result = await _authService.ChangePasswordAsync(User, request.CurrentPassword, request.NewPassword);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Errors);
            }
            return Ok();
        }

        [Authorize]
        [HttpPost("email-confirmation")]
        public async Task<IActionResult> GenerateEmailConfirmation()
        {
            var result = await _authService.GenerateEmailConfirmationAsync(User);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Errors);
            }
            return Ok(result.Value);
        }

        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromBody] EmailConfirmationRequest request)
        {
            var result = await _authService.ConfirmEmailAsync(request);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Errors);
            }
            return Ok();
        }

        [HttpPost("request-password-reset")]
        public async Task<IActionResult> RequestPasswordReset([FromBody] string email)
        {
            var result = await _authService.GeneratePasswordResetTokenAsync(email);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Errors);
            }
            return Ok();
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] PasswordResetRequest request)
        {
            var result = await _authService.ResetPasswordAsync(request);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Errors);
            }
            return Ok();
        }

        [Authorize]
        [HttpPost("request-email-change")]
        public async Task<IActionResult> RequestEmailChange([FromBody] EmailChangeRequest request)
        {
            var result = await _authService.GenerateEmailChangeAsync(User, request.NewEmail);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Errors);
            }
            return Ok(result.Value);
        }

        [HttpGet("status")]
        public async Task<IActionResult> CheckAuthStatus()
        {
            try
            {
                if (!User.Identity!.IsAuthenticated)
                {
                    return Ok(new { success = false });
                }

                // More robust user lookup
                var nameIdentifier = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var email = User.FindFirstValue(ClaimTypes.Email);

                // Try to find user by external login first
                var user = await FindUserByExternalLogin(nameIdentifier);

                // If not found, try by email
                if (user == null && !string.IsNullOrEmpty(email))
                {
                    user = await _userManager.FindByEmailAsync(email);
                }

                if (user == null)
                {
                    // Attempt to create user if not exists
                    user = await CreateUserFromClaims();
                }

                if (user == null)
                {
                    // Force sign out if no user can be found or created
                    await _signInManager.SignOutAsync();
                    return Ok(new { success = false });
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
                // Log the full exception
                Console.WriteLine($"Detailed Error in CheckAuthStatus: {ex}");
                return Ok(new { success = false, error = ex.Message });
            }
        }

        private async Task<AppUser> FindUserByExternalLogin(string nameIdentifier)
        {
            if (string.IsNullOrEmpty(nameIdentifier))
                return null;

            var users = _userManager.Users.ToList();
            foreach (var user in users)
            {
                var logins = await _userManager.GetLoginsAsync(user);
                if (logins.Any(l => l.ProviderKey == nameIdentifier))
                {
                    return user;
                }
            }
            return null;
        }

        private async Task<AppUser> CreateUserFromClaims()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var firstName = User.FindFirstValue(ClaimTypes.GivenName);
            var lastName = User.FindFirstValue(ClaimTypes.Surname);
            var nameIdentifier = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(email))
                return null;

            var user = new AppUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName = firstName ?? "",
                LastName = lastName ?? ""
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                Console.WriteLine($"Failed to create user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                return null;
            }

            // If we have a name identifier, add the external login
            if (!string.IsNullOrEmpty(nameIdentifier))
            {
                var loginInfo = new UserLoginInfo("Microsoft", nameIdentifier, null);
                var addLoginResult = await _userManager.AddLoginAsync(user, loginInfo);
                if (!addLoginResult.Succeeded)
                {
                    Console.WriteLine($"Failed to add external login: {string.Join(", ", addLoginResult.Errors.Select(e => e.Description))}");
                }
            }

            return user;
        }
    }
}
