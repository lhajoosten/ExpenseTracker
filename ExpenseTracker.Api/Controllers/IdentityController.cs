using ExpenseTracker.Common.Abstractions;
using ExpenseTracker.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class IdentityController(IAuthService authService) : ControllerBase
    {
        private readonly IAuthService _authService = authService;

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
    }
}
