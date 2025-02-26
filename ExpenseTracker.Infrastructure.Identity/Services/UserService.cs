using ExpenseTracker.Common.Abstractions;
using ExpenseTracker.Common.Models;
using ExpenseTracker.Infrastructure.Identity.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace ExpenseTracker.Infrastructure.Identity.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<AppUser> _userManager;

        public UserService(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<Result> AddPasswordAsync(ClaimsPrincipal principal, string newPassword)
        {
            var user = await _userManager.GetUserAsync(principal);
            if (user == null)
                return Result.Failure(new Dictionary<string, string> { { "User", "User not found" } });

            var result = await _userManager.AddPasswordAsync(user, newPassword);
            return result.Succeeded ? Result.Success() : Result.Failure(result.Errors.ToDictionary(e => e.Code, e => e.Description));
        }

        public async Task<Result> ChangeEmailAsync(ClaimsPrincipal principal, string email, string code)
        {
            var user = await _userManager.GetUserAsync(principal);
            if (user == null)
                return Result.Failure(new Dictionary<string, string> { { "User", "User not found" } });

            var result = await _userManager.ChangeEmailAsync(user, email, code);
            return result.Succeeded ? Result.Success() : Result.Failure(result.Errors.ToDictionary(e => e.Code, e => e.Description));
        }

        public async Task<Result> ChangePasswordAsync(ClaimsPrincipal principal, string oldPassword, string newPassword)
        {
            var user = await _userManager.GetUserAsync(principal);
            if (user == null)
                return Result.Failure(new Dictionary<string, string> { { "User", "User not found" } });

            var result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
            return result.Succeeded ? Result.Success() : Result.Failure(result.Errors.ToDictionary(e => e.Code, e => e.Description));
        }

        public async Task<Result<ApplicationUserDto>> FindByEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Result<ApplicationUserDto>.Failure(new Dictionary<string, string> { { "User", "User not found" } });

            var appUserRoles = await _userManager.GetRolesAsync(user);
            if (appUserRoles == null)
                return Result<ApplicationUserDto>.Failure(new Dictionary<string, string> { { "Roles", "Roles not found" } });

            var userDto = new ApplicationUserDto
            {
                Id = user.Id,
                UserName = user.UserName!,
                Email = user.Email!,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumber = user.PhoneNumber!,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                Roles = [.. appUserRoles.Select(r => new ApplicationRoleDto { Name = r })]
            };

            return Result<ApplicationUserDto>.Success(userDto);
        }

        public async Task<Result<ApplicationUserDto>> FindByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Result<ApplicationUserDto>.Failure(new Dictionary<string, string> { { "User", "User not found" } });

            var appUserRoles = await _userManager.GetRolesAsync(user);
            if (appUserRoles == null)
                return Result<ApplicationUserDto>.Failure(new Dictionary<string, string> { { "Roles", "Roles not found" } });

            var userDto = new ApplicationUserDto
            {
                Id = user.Id,
                UserName = user.UserName!,
                Email = user.Email!,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumber = user.PhoneNumber!,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                Roles = [.. appUserRoles.Select(r => new ApplicationRoleDto { Name = r })]
            };

            return Result<ApplicationUserDto>.Success(userDto);
        }

        public async Task<Result<string>> GetEmailAsync(ClaimsPrincipal principal)
        {
            var user = await _userManager.GetUserAsync(principal);
            if (user == null)
                return Result<string>.Failure(new Dictionary<string, string> { { "User", "User not found" } });

            return Result<string>.Success(user.Email!);
        }

        public async Task<Result<string>> GetPhoneNumberAsync(ClaimsPrincipal principal)
        {
            var user = await _userManager.GetUserAsync(principal);
            if (user == null)
                return Result<string>.Failure(new Dictionary<string, string> { { "User", "User not found" } });

            return Result<string>.Success(user.PhoneNumber!);
        }

        public async Task<Result<string>> GetUserIdAsync(ClaimsPrincipal principal)
        {
            var user = await _userManager.GetUserAsync(principal);
            if (user == null)
                return Result<string>.Failure(new Dictionary<string, string> { { "User", "User not found" } });

            return Result<string>.Success(user.Id.ToString());
        }

        public async Task<Result<string>> GetUserNameAsync(ClaimsPrincipal principal)
        {
            var user = await _userManager.GetUserAsync(principal);
            if (user == null)
                return Result<string>.Failure(new Dictionary<string, string> { { "User", "User not found" } });

            return Result<string>.Success(user.UserName!);
        }

        public async Task<Result> HasPasswordAsync(ClaimsPrincipal principal)
        {
            var user = await _userManager.GetUserAsync(principal);
            if (user == null)
                return Result.Failure(new Dictionary<string, string> { { "User", "User not found" } });

            var hasPassword = await _userManager.HasPasswordAsync(user);
            return hasPassword ? Result.Success() : Result.Failure(new Dictionary<string, string> { { "Password", "User does not have a password" } });
        }

        public async Task<Result> IsEmailConfirmedAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Result.Failure(new Dictionary<string, string> { { "User", "User not found" } });

            return user.EmailConfirmed ? Result.Success() : Result.Failure(new Dictionary<string, string> { { "Email", "Email not confirmed" } });
        }

        public async Task<Result> SetPhoneNumberAsync(ClaimsPrincipal principal, string phoneNumber)
        {
            var user = await _userManager.GetUserAsync(principal);
            if (user == null)
                return Result.Failure(new Dictionary<string, string> { { "User", "User not found" } });

            var result = await _userManager.SetPhoneNumberAsync(user, phoneNumber);
            return result.Succeeded ? Result.Success() : Result.Failure(result.Errors.ToDictionary(e => e.Code, e => e.Description));
        }
    }
}