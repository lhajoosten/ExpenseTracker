using ExpenseTracker.Common.Models;
using ExpenseTracker.Common.Models.Identity;
using System.Security.Claims;

namespace ExpenseTracker.Common.Abstractions.Identity
{
    public interface IAuthService
    {
        // Register a new user
        Task<Result> SignUpAsync(SignUpRequest signUpRequest);

        // Login as a newly registered user
        Task<Result> SignInAsync(SignInRequest signInRequest);

        // Confirm email (this will send a confirm-email email)
        Task<Result<TokenResponse>> GenerateEmailConfirmationAsync(ClaimsPrincipal user);
        Task<Result> ConfirmEmailAsync(EmailConfirmationRequest emailConfirmationRequest);

        // Request a password reset (this will send a reset-password email)
        Task<Result> GeneratePasswordResetTokenAsync(string email);
        Task<Result> ResetPasswordAsync(PasswordResetRequest resetPasswordRequest);

        // Change password with the password reset
        Task<Result> ChangePasswordAsync(ClaimsPrincipal user, string currentPassword, string newPassword);

        // Request an email reset (this will send a reset-email email)
        Task<Result<TokenResponse>> GenerateEmailChangeAsync(ClaimsPrincipal user, string newEmail);

        // Refresh sign-in session
        Task RefreshSignInAsync(ClaimsPrincipal user);

        // Logout from session
        Task SignOutAsync();

        // Get current user details
        Task<Result<ApplicationUserDto>> GetCurrentUserAsync(ClaimsPrincipal user);
    }
}
