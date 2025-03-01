using ExpenseTracker.Common.Abstractions;
using ExpenseTracker.Common.Models;
using ExpenseTracker.Infrastructure.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace ExpenseTracker.Infrastructure.Identity.Services
{
    public class AuthService(
        UserManager<AppUser> userManager, 
        SignInManager<AppUser> signInManager, 
        IEmailService emailSender, 
        ILogger<AuthService> logger) : IAuthService
    {
        private readonly UserManager<AppUser> _userManager = userManager;
        private readonly SignInManager<AppUser> _signInManager = signInManager;
        private readonly IEmailService _emailSender = emailSender;
        private readonly ILogger<AuthService> _logger = logger;

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
            _logger.LogInformation("Attempting to get current user.");

            AppUser? user = null;

            try
            {
                // Try to get the user directly - but this might fail with external login providers
                user = await _userManager.GetUserAsync(principal);
                _logger.LogInformation("User retrieved directly using GetUserAsync.");
            }
            catch (FormatException ex)
            {
                // This is expected with external login providers that have non-GUID NameIdentifier formats
                _logger.LogInformation("Format exception in GetUserAsync (expected with OAuth): {Message}", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Unexpected error in GetUserAsync: {Message}", ex.Message);
            }

            // If direct lookup fails, try by name claim
            if (user == null && principal.Identity?.Name != null)
            {
                _logger.LogInformation("Attempting to find user by name claim: {Name}", principal.Identity.Name);
                user = await _userManager.FindByNameAsync(principal.Identity.Name);
            }

            // If that fails too, try by email claim
            if (user == null)
            {
                var email = principal.FindFirstValue(ClaimTypes.Email);
                if (!string.IsNullOrEmpty(email))
                {
                    _logger.LogInformation("Attempting to find user by email claim: {Email}", email);
                    user = await _userManager.FindByEmailAsync(email);
                }
            }

            // As a last resort, try by sub claim (common in OAuth flows)
            if (user == null)
            {
                var nameIdentifier = principal.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(nameIdentifier))
                {
                    _logger.LogInformation("Attempting to find user by NameIdentifier claim: {NameIdentifier}", nameIdentifier);
                    // Try to find user by external login
                    var users = _userManager.Users.ToList();
                    foreach (var potentialUser in users)
                    {
                        var logins = await _userManager.GetLoginsAsync(potentialUser);
                        if (logins.Any(l => l.ProviderKey == nameIdentifier))
                        {
                            user = potentialUser;
                            _logger.LogInformation("User found by external login: {UserId}", user.Id);
                            break;
                        }
                    }
                }
            }

            if (user == null)
            {
                _logger.LogWarning("User not found.");
                return Result<ApplicationUserDto>.Failure(new Dictionary<string, string>
        {
            { "User", "User not found." }
        });
            }

            _logger.LogInformation("User found: {UserId}", user.Id);

            var roles = await _userManager.GetRolesAsync(user);
            var rolesDto = roles.Select(r => new ApplicationRoleDto { Name = r }).ToList();

            var userDto = new ApplicationUserDto
            {
                Id = user.Id, // No need to parse this since it's already a Guid
                UserName = user.UserName!,
                Firstname = user.FirstName ?? string.Empty,
                Lastname = user.LastName ?? string.Empty,
                Email = user.Email!,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                Roles = rolesDto
            };

            _logger.LogInformation("Returning user details for user: {UserId}", user.Id);

            return Result<ApplicationUserDto>.Success(userDto);
        }
    }
}