using ExpenseTracker.Common.Models;
using System.Security.Claims;

namespace ExpenseTracker.Common.Abstractions
{
    public interface IAuthService
    {
        Task<Result> SignInAsync(SignInRequest signInRequest);
        Task SignOutAsync();
        Task<Result> SignUpAsync(SignUpRequest signUpRequest);
        Task<Result> ChangePasswordAsync(ClaimsPrincipal user, string currentPassword, string newPassword);
        Task<Result> ResetPasswordAsync(ResetPasswordRequest resetPasswordRequest);
        Task<Result<TokenResponse>> GeneratePasswordResetTokenAsync(string email);
        Task<Result<ApplicationUserDto>> GetCurrentUserAsync(ClaimsPrincipal user);
        Task<Result<TokenResponse>> GenerateEmailConfirmationAsync(ClaimsPrincipal user);
        Task<Result<TokenResponse>> GenerateEmailChangeAsync(ClaimsPrincipal user, string newEmail);
        Task<Result> ConfirmEmailAsync(EmailConfirmationRequest emailConfirmationRequest);
        Task RefreshSignInAsync(ClaimsPrincipal user);
    }
}
