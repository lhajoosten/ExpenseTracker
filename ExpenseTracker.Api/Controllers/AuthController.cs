using ExpenseTracker.Common.Abstractions;
using ExpenseTracker.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        private readonly IAuthService _authService = authService;

        [HttpPost("signin")]
        public async Task<IActionResult> SignIn([FromBody] SignInRequest signInRequest)
        {
            var result = await _authService.SignInAsync(signInRequest);
            if (result.IsSuccess)
                return Ok(result);
            return BadRequest(result.Errors);
        }

        [HttpPost("signout")]
        [Authorize]
        public async Task<IActionResult> SignOut()
        {
            await _authService.SignOutAsync();
            return NoContent();
        }

        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] SignUpRequest signUpRequest)
        {
            var result = await _authService.SignUpAsync(signUpRequest);
            if (result.IsSuccess)
                return Ok(result);
            return BadRequest(result.Errors);
        }

        [HttpPost("changepassword")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest changePasswordRequest)
        {
            var result = await _authService.ChangePasswordAsync(User, changePasswordRequest.CurrentPassword, changePasswordRequest.NewPassword);
            if (result.IsSuccess)
                return Ok(result);
            return BadRequest(result.Errors);
        }

        [HttpPost("resetpassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest resetPasswordRequest)
        {
            var result = await _authService.ResetPasswordAsync(resetPasswordRequest);
            if (result.IsSuccess)
                return Ok(result);
            return BadRequest(result.Errors);
        }

        [HttpPost("generatepasswordresettoken")]
        public async Task<IActionResult> GeneratePasswordResetToken([FromBody] string email)
        {
            var result = await _authService.GeneratePasswordResetTokenAsync(email);
            if (result.IsSuccess)
                return Ok(result);
            return BadRequest(result.Errors);
        }

        [HttpGet("currentuser")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var result = await _authService.GetCurrentUserAsync(User);
            if (result.IsSuccess)
                return Ok(result);
            return BadRequest(result.Errors);
        }

        [HttpPost("generateemailconfirmation")]
        [Authorize]
        public async Task<IActionResult> GenerateEmailConfirmation()
        {
            var result = await _authService.GenerateEmailConfirmationAsync(User);
            if (result.IsSuccess)
                return Ok(result);
            return BadRequest(result.Errors);
        }

        [HttpPost("generateemailchange")]
        [Authorize]
        public async Task<IActionResult> GenerateEmailChange([FromBody] string newEmail)
        {
            var result = await _authService.GenerateEmailChangeAsync(User, newEmail);
            if (result.IsSuccess)
                return Ok(result);
            return BadRequest(result.Errors);
        }

        [HttpPost("confirmemail")]
        public async Task<IActionResult> ConfirmEmail([FromBody] EmailConfirmationRequest emailConfirmationRequest)
        {
            var result = await _authService.ConfirmEmailAsync(emailConfirmationRequest);
            if (result.IsSuccess)
                return Ok(result);
            return BadRequest(result.Errors);
        }

        [HttpPost("refreshsignin")]
        [Authorize]
        public async Task<IActionResult> RefreshSignIn()
        {
            await _authService.RefreshSignInAsync(User);
            return NoContent();
        }
    }
}
