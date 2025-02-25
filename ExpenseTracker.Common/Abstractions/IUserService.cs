using ExpenseTracker.Common.Models;
using System.Security.Claims;

namespace ExpenseTracker.Common.Abstractions
{
    public interface IUserService
    {
        Task<Result<ApplicationUserDto>> FindByIdAsync(string userId);
        Task<Result<ApplicationUserDto>> FindByEmailAsync(string email);
        Task<Result<string>> GetUserIdAsync(ClaimsPrincipal principal);
        Task<Result<string>> GetEmailAsync(ClaimsPrincipal principal);
        Task<Result<string>> GetUserNameAsync(ClaimsPrincipal principal);
        Task<Result<string>> GetPhoneNumberAsync(ClaimsPrincipal principal);
        Task<Result> ChangeEmailAsync(ClaimsPrincipal principal, string email, string code);
        Task<Result> ChangePasswordAsync(ClaimsPrincipal principal, string oldPassword, string newPassword);
        Task<Result> IsEmailConfirmedAsync(string email);
        Task<Result> HasPasswordAsync(ClaimsPrincipal principal);
        Task<Result> SetPhoneNumberAsync(ClaimsPrincipal principal, string phoneNumber);
        Task<Result> AddPasswordAsync(ClaimsPrincipal principal, string newPassword);
    }
}
