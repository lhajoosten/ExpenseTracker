using ExpenseTracker.Common.Abstractions;
using ExpenseTracker.Common.Models;
using ExpenseTracker.Infrastructure.Identity.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace ExpenseTracker.Infrastructure.Identity.Services
{
    public class AuthService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IEmailService emailSender) : IAuthService
    {
        private readonly UserManager<AppUser> _userManager = userManager;
        private readonly SignInManager<AppUser> _signInManager = signInManager;
        private readonly IEmailService _emailSender = emailSender;

        public async Task<Result> SignUpAsync(SignUpRequest signUpRequest)
        {
            if (signUpRequest.Password != signUpRequest.ConfirmPassword)
            {
                return Result.Failure(new Dictionary<string, string>
                {
                    { "Password", "Passwords do not match." }
                });
            }

            var user = new AppUser
            {
                UserName = signUpRequest.Email,
                Email = signUpRequest.Email,
                FirstName = signUpRequest.Firstname,
                LastName = signUpRequest.Lastname
            };

            var identityResult = await _userManager.CreateAsync(user, signUpRequest.Password);
            if (!identityResult.Succeeded)
            {
                var errors = identityResult.Errors.ToDictionary(e => e.Code, e => e.Description);
                return Result.Failure(errors);
            }

            var roleResult = await _userManager.AddToRoleAsync(user, "User");
            if (!roleResult.Succeeded)
            {
                var errors = roleResult.Errors.ToDictionary(e => e.Code, e => e.Description);
                return Result.Failure(errors);
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            await _emailSender.SendEmailConfirmationAsync(user.Email!, token);

            return Result.Success();
        }

        public async Task<Result> SignInAsync(SignInRequest signInRequest)
        {
            var user = await _userManager.FindByEmailAsync(signInRequest.Email);
            if (user == null)
            {
                return Result.Failure(new Dictionary<string, string>
                {
                    { "User", "User not found." }
                });
            }

            var signInResult = await _signInManager.PasswordSignInAsync(user, signInRequest.Password, signInRequest.RememberMe, lockoutOnFailure: false);
            if (!signInResult.Succeeded)
            {
                return Result.Failure(new Dictionary<string, string>
                {
                    { "SignIn", "Invalid login attempt." }
                });
            }

            return Result.Success();
        }

        public async Task SignOutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<Result<TokenResponse>> GenerateEmailConfirmationAsync(ClaimsPrincipal principal)
        {
            var user = await _userManager.GetUserAsync(principal);
            if (user == null)
            {
                return Result<TokenResponse>.Failure(new Dictionary<string, string>
                {
                    { "User", "User not found." }
                });
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var emailResult = await _emailSender.SendEmailConfirmationAsync(user.Email!, token);
            if (!emailResult.IsSuccess)
            {
                return Result<TokenResponse>.Failure(new Dictionary<string, string>
                {
                    { "Email", "Failed to send confirmation email." }
                });
            }

            var tokenResponse = new TokenResponse
            {
                UserId = Guid.Parse(user.Id.ToString()),
                Token = token,
                RefreshToken = string.Empty,
                Expires = DateTime.UtcNow.AddHours(24)
            };

            return Result<TokenResponse>.Success(tokenResponse);
        }

        public async Task<Result> ConfirmEmailAsync(EmailConfirmationRequest emailConfirmationRequest)
        {
            var user = await _userManager.FindByIdAsync(emailConfirmationRequest.UserId);
            if (user == null)
            {
                return Result.Failure(new Dictionary<string, string>
                {
                    { "User", "User not found." }
                });
            }

            var result = await _userManager.ConfirmEmailAsync(user, emailConfirmationRequest.Token);
            if (!result.Succeeded)
            {
                var errors = result.Errors.ToDictionary(e => e.Code, e => e.Description);
                return Result.Failure(errors);
            }

            return Result.Success();
        }

        public async Task<Result> GeneratePasswordResetTokenAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Result.Failure(new Dictionary<string, string>
                {
                    { "User", "User not found." }
                });
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var emailResult = await _emailSender.SendPasswordResetAsync(email, token);
            if (!emailResult.IsSuccess)
            {
                return Result.Failure(new Dictionary<string, string>
                {
                    { "Email", "Failed to send password reset email." }
                });
            }

            return Result.Success();
        }

        public async Task<Result> ResetPasswordAsync(PasswordResetRequest resetPasswordRequest)
        {
            var user = await _userManager.FindByEmailAsync(resetPasswordRequest.Email);
            if (user == null)
            {
                return Result.Failure(new Dictionary<string, string>
                {
                    { "User", "User not found." }
                });
            }

            var result = await _userManager.ResetPasswordAsync(user, resetPasswordRequest.Token, resetPasswordRequest.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.ToDictionary(e => e.Code, e => e.Description);
                return Result.Failure(errors);
            }

            return Result.Success();
        }

        public async Task<Result> ChangePasswordAsync(ClaimsPrincipal principal, string currentPassword, string newPassword)
        {
            var user = await _userManager.GetUserAsync(principal);
            if (user == null)
            {
                return Result.Failure(new Dictionary<string, string>
                {
                    { "User", "User not found." }
                });
            }

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            if (!result.Succeeded)
            {
                var errors = result.Errors.ToDictionary(e => e.Code, e => e.Description);
                return Result.Failure(errors);
            }

            return Result.Success();
        }

        public async Task<Result<TokenResponse>> GenerateEmailChangeAsync(ClaimsPrincipal principal, string newEmail)
        {
            var user = await _userManager.GetUserAsync(principal);
            if (user == null)
            {
                return Result<TokenResponse>.Failure(new Dictionary<string, string>
                {
                    { "User", "User not found." }
                });
            }

            var token = await _userManager.GenerateChangeEmailTokenAsync(user, newEmail);
            var emailResult = await _emailSender.SendEmailConfirmationAsync(newEmail, token);
            if (!emailResult.IsSuccess)
            {
                return Result<TokenResponse>.Failure(new Dictionary<string, string>
                {
                    { "Email", "Failed to send email change confirmation email." }
                });
            }

            var tokenResponse = new TokenResponse
            {
                UserId = Guid.Parse(user.Id.ToString()),
                Token = token,
                RefreshToken = string.Empty,
                Expires = DateTime.UtcNow.AddHours(24) // Example expiration
            };

            return Result<TokenResponse>.Success(tokenResponse);
        }

        public async Task RefreshSignInAsync(ClaimsPrincipal principal)
        {
            var user = await _userManager.GetUserAsync(principal);
            if (user != null)
            {
                await _signInManager.RefreshSignInAsync(user);
            }
        }

        public async Task<Result<ApplicationUserDto>> GetCurrentUserAsync(ClaimsPrincipal principal)
        {
            var user = await _userManager.GetUserAsync(principal);
            if (user == null)
            {
                return Result<ApplicationUserDto>.Failure(new Dictionary<string, string>
                {
                    { "User", "User not found." }
                });
            }

            var roles = await _userManager.GetRolesAsync(user);
            var rolesDto = roles.Select(r => new ApplicationRoleDto { Name = r }).ToList();

            var userDto = new ApplicationUserDto
            {
                Id = Guid.Parse(user.Id.ToString()),
                UserName = user.UserName!,
                Email = user.Email!,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumber = user.PhoneNumber!,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                Roles = rolesDto
            };

            return Result<ApplicationUserDto>.Success(userDto);
        }
    }
}