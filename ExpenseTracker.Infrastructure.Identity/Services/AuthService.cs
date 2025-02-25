using ExpenseTracker.Common.Abstractions;
using ExpenseTracker.Common.Models;
using ExpenseTracker.Infrastructure.Identity.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace ExpenseTracker.Infrastructure.Identity.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IEmailService _emailSender;

        public AuthService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IEmailService emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        public async Task<Result> ChangePasswordAsync(ClaimsPrincipal user, string currentPassword, string newPassword)
        {
            var appUser = await _userManager.GetUserAsync(user);
            if (appUser == null)
                return Result.Failure(new Dictionary<string, string> { { "User", "User not found" } });

            var result = await _userManager.ChangePasswordAsync(appUser, currentPassword, newPassword);
            return result.Succeeded ? Result.Success() : Result.Failure(result.Errors.ToDictionary(e => e.Code, e => e.Description));
        }

        public async Task<Result> ConfirmEmailAsync(EmailConfirmationRequest emailConfirmationRequest)
        {
            var user = await _userManager.FindByIdAsync(emailConfirmationRequest.UserId);
            if (user == null)
                return Result.Failure(new Dictionary<string, string> { { "User", "User not found" } });

            var result = await _userManager.ConfirmEmailAsync(user, emailConfirmationRequest.Token);
            return result.Succeeded ? Result.Success() : Result.Failure(result.Errors.ToDictionary(e => e.Code, e => e.Description));
        }

        public async Task<Result<TokenResponse>> GenerateEmailChangeAsync(ClaimsPrincipal user, string newEmail)
        {
            var appUser = await _userManager.GetUserAsync(user);
            if (appUser == null)
                return Result<TokenResponse>.Failure(new Dictionary<string, string> { { "User", "User not found" } });

            var token = await _userManager.GenerateChangeEmailTokenAsync(appUser, newEmail);
            return Result<TokenResponse>.Success(new TokenResponse { UserId = appUser.Id, Token = token });
        }

        public async Task<Result<TokenResponse>> GenerateEmailConfirmationAsync(ClaimsPrincipal user)
        {
            var appUser = await _userManager.GetUserAsync(user);
            if (appUser == null)
                return Result<TokenResponse>.Failure(new Dictionary<string, string> { { "User", "User not found" } });

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(appUser);
            return Result<TokenResponse>.Success(new TokenResponse { UserId = appUser.Id, Token = token });
        }

        public async Task<Result<TokenResponse>> GeneratePasswordResetTokenAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Result<TokenResponse>.Failure(new Dictionary<string, string> { { "User", "User not found" } });

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            return Result<TokenResponse>.Success(new TokenResponse { UserId = user.Id, Token = token });
        }

        public async Task<Result<ApplicationUserDto>> GetCurrentUserAsync(ClaimsPrincipal user)
        {
            var appUser = await _userManager.GetUserAsync(user);
            if (appUser == null)
                return Result<ApplicationUserDto>.Failure(new Dictionary<string, string> { { "User", "User not found" } });

            var userDto = new ApplicationUserDto
            {
                Id = appUser.Id,
                UserName = appUser.UserName!,
                Email = appUser.Email!,
                EmailConfirmed = appUser.EmailConfirmed,
                PhoneNumber = appUser.PhoneNumber!,
                PhoneNumberConfirmed = appUser.PhoneNumberConfirmed,
                Role = new ApplicationRoleDto { Id = appUser.Role.Id, Name = appUser.Role.Name! }
            };

            return Result<ApplicationUserDto>.Success(userDto);
        }

        public async Task RefreshSignInAsync(ClaimsPrincipal user)
        {
            var appUser = await _userManager.GetUserAsync(user);
            if (appUser != null)
            {
                await _signInManager.RefreshSignInAsync(appUser);
            }
        }

        public async Task<Result> ResetPasswordAsync(ResetPasswordRequest resetPasswordRequest)
        {
            var user = await _userManager.FindByEmailAsync(resetPasswordRequest.Email);
            if (user == null)
                return Result.Failure(new Dictionary<string, string> { { "User", "User not found" } });

            var result = await _userManager.ResetPasswordAsync(user, resetPasswordRequest.Token, resetPasswordRequest.Password);
            return result.Succeeded ? Result.Success() : Result.Failure(result.Errors.ToDictionary(e => e.Code, e => e.Description));
        }

        public async Task<Result> SignInAsync(SignInRequest signInRequest)
        {
            var result = await _signInManager.PasswordSignInAsync(signInRequest.Email, signInRequest.Password, signInRequest.RememberMe, lockoutOnFailure: false);
            return result.Succeeded ? Result.Success() : Result.Failure(new Dictionary<string, string> { { "SignIn", "Invalid login attempt" } });
        }

        public async Task SignOutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<Result> SignUpAsync(SignUpRequest signUpRequest)
        {
            var user = new AppUser
            {
                UserName = signUpRequest.Email,
                Email = signUpRequest.Email,
                FirstName = signUpRequest.Firstname,
                LastName = signUpRequest.Lastname,
                RoleId = Guid.Parse("a8e118e7-30f1-44be-be22-0d4007b524be") // TO-DO :: need to automate this to user role automatically for new users
            };

            var result = await _userManager.CreateAsync(user, signUpRequest.Password);
            if (!result.Succeeded)
                return Result.Failure(result.Errors.ToDictionary(e => e.Code, e => e.Description));

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            await _emailSender.SendEmailAsync(signUpRequest.Email, "Confirm your email", $"Please confirm your account by using this token: {token}");

            return Result.Success();
        }
    }
}