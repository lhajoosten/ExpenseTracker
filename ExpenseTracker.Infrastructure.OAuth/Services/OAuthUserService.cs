using ExpenseTracker.Common.Abstractions.OAuth;
using ExpenseTracker.Common.Models;
using ExpenseTracker.Common.Models.OAuth;
using ExpenseTracker.Infrastructure.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace ExpenseTracker.Infrastructure.OAuth.Services
{
    public class OAuthUserService(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        IOAuthProviderService providerService,
        ILogger<OAuthUserService> logger) : IOAuthUserService
    {
        private readonly UserManager<AppUser> _userManager = userManager;
        private readonly SignInManager<AppUser> _signInManager = signInManager;
        private readonly IOAuthProviderService _providerService = providerService;
        private readonly ILogger<OAuthUserService> _logger = logger;

        public async Task<OAuthLoginResult> GetCurrentUserStatusAsync(ClaimsPrincipal principal)
        {
            try
            {
                _logger.LogInformation("Getting current user status");

                var userInfo = GetUserInfoFromPrincipal(principal);
                if (userInfo == null)
                {
                    return OAuthLoginResult.Failure("", "User not found", "No valid user identity found in claims");
                }

                var appUser = await FindUserByIdentifier(userInfo);
                if (appUser == null)
                {
                    return OAuthLoginResult.Failure("", "User not found", "User could not be found in the database");
                }

                var roles = await _userManager.GetRolesAsync(appUser);

                var userDto = new ApplicationUserDto
                {
                    Id = appUser.Id,
                    Email = appUser.Email!,
                    UserName = appUser.UserName!,
                    Firstname = appUser.FirstName ?? "",
                    Lastname = appUser.LastName ?? "",
                    EmailConfirmed = appUser.EmailConfirmed,
                    Roles = roles.Select(r => new ApplicationRoleDto { Name = r }).ToList()
                };

                return OAuthLoginResult.Success("", userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user status");
                return OAuthLoginResult.Failure("", "Error retrieving user", ex.Message);
            }
        }

        public async Task<OAuthLoginResult> EnsureUserExistsAsync(ClaimsPrincipal principal)
        {
            try
            {
                _logger.LogInformation("Ensuring user exists");

                // Get user info and provider
                var userInfo = GetUserInfoFromPrincipal(principal);
                if (userInfo == null)
                {
                    return OAuthLoginResult.Failure("", "User not found", "No valid user identity found in claims");
                }

                var provider = _providerService.DetermineProvider(principal);
                _logger.LogInformation("Provider determined: {Provider}", provider);

                // Find or create user
                AppUser user = null;

                // First try by email
                if (!string.IsNullOrEmpty(userInfo.Email))
                {
                    user = await _userManager.FindByEmailAsync(userInfo.Email);
                    if (user != null)
                    {
                        _logger.LogInformation("User found by email: {Email}, UserID: {UserId}",
                            userInfo.Email, user.Id);
                    }
                }

                // Then try by external login
                if (user == null && !string.IsNullOrEmpty(userInfo.NameIdentifier))
                {
                    user = await FindUserByExternalLoginInternalAsync(userInfo.NameIdentifier);
                    if (user != null)
                    {
                        _logger.LogInformation("User found by external login: {UserId}", user.Id);
                    }
                }

                // Create user if not found
                if (user == null)
                {
                    _logger.LogInformation("User not found, creating new user");

                    var (firstName, lastName) = _providerService.ExtractNameFromClaims(principal, provider);

                    user = new AppUser
                    {
                        UserName = userInfo.Email,
                        Email = userInfo.Email,
                        EmailConfirmed = true, // Auto-confirm for OAuth users
                        FirstName = firstName,
                        LastName = lastName
                    };

                    var createResult = await _userManager.CreateAsync(user);
                    if (!createResult.Succeeded)
                    {
                        var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                        _logger.LogError("Failed to create user: {Errors}", errors);
                        return OAuthLoginResult.Failure("", "User creation failed", errors);
                    }

                    _logger.LogInformation("User created with ID: {UserId}", user.Id);

                    // Add the user to the User role
                    var roleResult = await _userManager.AddToRoleAsync(user, "User");
                    if (!roleResult.Succeeded)
                    {
                        _logger.LogWarning("Failed to add user to role: {Errors}",
                            string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                    }
                }

                // Critical null check here before proceeding
                if (user == null)
                {
                    _logger.LogError("User is still null after creation attempt");
                    return OAuthLoginResult.Failure("", "User creation failed",
                        "Unable to find or create user account");
                }

                // Link provider and refresh sign-in
                if (!string.IsNullOrEmpty(userInfo.NameIdentifier))
                {
                    await EnsureProviderLinkedAsync(user.Id.ToString(), userInfo.NameIdentifier, provider);
                }

                await _signInManager.RefreshSignInAsync(user);

                // Return user info
                var roles = await _userManager.GetRolesAsync(user);
                var userDto = new ApplicationUserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    UserName = user.UserName,
                    Firstname = user.FirstName ?? "",
                    Lastname = user.LastName ?? "",
                    EmailConfirmed = user.EmailConfirmed,
                    Roles = roles.Select(r => new ApplicationRoleDto { Name = r }).ToList()
                };

                return OAuthLoginResult.Success("", userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ensuring user exists");
                return OAuthLoginResult.Failure("", "Error ensuring user exists", ex.Message);
            }
        }

        public async Task<OAuthUserInfo?> GetUserFromExternalLoginAsync(string nameIdentifier, string provider)
        {
            try
            {
                _logger.LogInformation("Getting user from external login - NameIdentifier: {NameIdentifier}, Provider: {Provider}",
                    nameIdentifier, provider);

                var appUser = await FindUserByExternalLoginInternalAsync(nameIdentifier);
                if (appUser == null)
                {
                    return null;
                }

                return new OAuthUserInfo
                {
                    NameIdentifier = nameIdentifier,
                    Email = appUser.Email!,
                    FirstName = appUser.FirstName ?? "",
                    LastName = appUser.LastName ?? "",
                    DisplayName = appUser.UserName!,
                    Provider = provider
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user from external login");
                return null;
            }
        }

        public async Task<OAuthUserInfo?> CreateOrUpdateUserFromClaimsAsync(ClaimsPrincipal principal, string provider)
        {
            try
            {
                var email = principal.FindFirstValue(ClaimTypes.Email);
                var nameIdentifier = principal.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(email))
                {
                    _logger.LogWarning("Cannot create user without email claim");
                    return null;
                }

                _logger.LogInformation("Creating or updating user from claims - Email: {Email}, Provider: {Provider}",
                    email, provider);

                // Extract name information
                var (firstName, lastName) = _providerService.ExtractNameFromClaims(principal, provider);

                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(email);
                if (existingUser != null)
                {
                    _logger.LogInformation("User with email {Email} already exists", email);

                    // Update user information if needed
                    bool userUpdated = false;

                    if (string.IsNullOrEmpty(existingUser.FirstName) && !string.IsNullOrEmpty(firstName))
                    {
                        existingUser.FirstName = firstName;
                        userUpdated = true;
                    }

                    if (string.IsNullOrEmpty(existingUser.LastName) && !string.IsNullOrEmpty(lastName))
                    {
                        existingUser.LastName = lastName;
                        userUpdated = true;
                    }

                    if (userUpdated)
                    {
                        await _userManager.UpdateAsync(existingUser);
                    }

                    // Add the external login
                    if (!string.IsNullOrEmpty(nameIdentifier))
                    {
                        await EnsureProviderLinkedAsync(existingUser.Id.ToString(), nameIdentifier, provider);
                    }

                    return new OAuthUserInfo
                    {
                        NameIdentifier = nameIdentifier ?? "",
                        Email = existingUser.Email!,
                        FirstName = existingUser.FirstName ?? "",
                        LastName = existingUser.LastName ?? "",
                        DisplayName = existingUser.UserName!,
                        Provider = provider
                    };
                }

                // Create new user
                var user = new AppUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true, // Auto-confirm for OAuth users
                    FirstName = firstName,
                    LastName = lastName
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    _logger.LogError("Failed to create user: {Errors}", errors);
                    return null;
                }

                // Add the user to the User role
                var roleResult = await _userManager.AddToRoleAsync(user, "User");
                if (!roleResult.Succeeded)
                {
                    var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                    _logger.LogWarning("Failed to add user to User role: {Errors}", errors);
                }

                // Add the external login
                if (!string.IsNullOrEmpty(nameIdentifier))
                {
                    await EnsureProviderLinkedAsync(user.Id.ToString(), nameIdentifier, provider);
                }

                return new OAuthUserInfo
                {
                    NameIdentifier = nameIdentifier ?? "",
                    Email = user.Email,
                    FirstName = user.FirstName ?? "",
                    LastName = user.LastName ?? "",
                    DisplayName = user.UserName,
                    Provider = provider
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating or updating user from claims");
                return null;
            }
        }

        public async Task EnsureProviderLinkedAsync(string userId, string nameIdentifier, string provider)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(nameIdentifier) || string.IsNullOrEmpty(provider))
            {
                throw new ArgumentException("UserId, nameIdentifier, and provider must not be null or empty");
            }

            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found with ID: {UserId}", userId);
                    return;
                }

                // Get existing logins
                var logins = await _userManager.GetLoginsAsync(user);

                // Log all existing logins for debugging
                foreach (var login in logins)
                {
                    _logger.LogDebug("User {UserId} has existing login: Provider={Provider}, Key={Key}",
                        userId, login.LoginProvider, login.ProviderKey);
                }

                // Check if this specific provider login already exists
                if (!logins.Any(l => l.LoginProvider == provider && l.ProviderKey == nameIdentifier))
                {
                    _logger.LogInformation("Adding {Provider} login to user {UserId}", provider, userId);

                    // Create display name
                    string displayName = $"{user.FirstName} {user.LastName}".Trim();
                    if (string.IsNullOrEmpty(displayName))
                    {
                        displayName = user.Email!;
                    }

                    // Add the login
                    var loginInfo = new UserLoginInfo(provider, nameIdentifier, displayName);
                    var result = await _userManager.AddLoginAsync(user, loginInfo);

                    if (!result.Succeeded)
                    {
                        _logger.LogWarning("Failed to add {Provider} login: {Errors}",
                            provider,
                            string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                    else
                    {
                        _logger.LogInformation("Successfully added {Provider} login to user {UserId}", provider, userId);
                    }
                }
                else
                {
                    _logger.LogInformation("{Provider} login already exists for user {UserId}", provider, userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ensuring provider {Provider} is linked to user {UserId}", provider, userId);
                throw;
            }
        }

        public async Task<OAuthUserInfo?> FindUserByExternalLoginAsync(string nameIdentifier)
        {
            try
            {
                _logger.LogInformation("Finding user by external login - NameIdentifier: {NameIdentifier}", nameIdentifier);

                if (string.IsNullOrEmpty(nameIdentifier))
                {
                    _logger.LogWarning("NameIdentifier is null or empty");
                    return null;
                }

                var appUser = await FindUserByExternalLoginInternalAsync(nameIdentifier);
                if (appUser == null)
                {
                    return null;
                }

                return new OAuthUserInfo
                {
                    NameIdentifier = nameIdentifier,
                    Email = appUser.Email!,
                    FirstName = appUser.FirstName ?? "",
                    LastName = appUser.LastName ?? "",
                    DisplayName = appUser.UserName!,
                    Provider = ""
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding user by external login");
                return null;
            }
        }

        // Helper methods
        private OAuthUserInfo? GetUserInfoFromPrincipal(ClaimsPrincipal principal)
        {
            var nameIdentifier = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = principal.FindFirstValue(ClaimTypes.Email);
            var name = principal.FindFirstValue(ClaimTypes.Name);

            if (string.IsNullOrEmpty(nameIdentifier) && string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("No valid identifier found in claims");
                return null;
            }

            var provider = _providerService.DetermineProvider(principal);
            var (firstName, lastName) = _providerService.ExtractNameFromClaims(principal, provider);

            return new OAuthUserInfo
            {
                NameIdentifier = nameIdentifier ?? "",
                Email = email ?? "",
                FirstName = firstName,
                LastName = lastName,
                DisplayName = name ?? email ?? "",
                Provider = provider
            };
        }

        private async Task<AppUser?> FindUserByIdentifier(OAuthUserInfo userInfo)
        {
            // Try to find user by external login first
            AppUser? user = null!;
            if (!string.IsNullOrEmpty(userInfo.NameIdentifier))
            {
                user = await FindUserByExternalLoginInternalAsync(userInfo.NameIdentifier);
                if (user != null)
                {
                    _logger.LogInformation("User found by external login: {UserId}", user.Id);
                    return user;
                }
            }

            // If not found, try by email
            if (user == null && !string.IsNullOrEmpty(userInfo.Email))
            {
                user = await _userManager.FindByEmailAsync(userInfo.Email);
                if (user != null)
                {
                    _logger.LogInformation("User found by email: {UserId}", user.Id);
                    return user;
                }
            }

            return null;
        }

        private async Task<AppUser> FindUserByExternalLoginInternalAsync(string nameIdentifier)
        {
            _logger.LogInformation("Finding user by external login name identifier: {NameIdentifier}", nameIdentifier);

            if (string.IsNullOrEmpty(nameIdentifier))
            {
                _logger.LogWarning("Name identifier is null or empty.");
                return null;
            }

            // Log all users and their logins
            var users = _userManager.Users.ToList();
            _logger.LogInformation("Total users in database: {UserCount}", users.Count);

            foreach (var user in users)
            {
                var logins = await _userManager.GetLoginsAsync(user);

                foreach (var login in logins)
                {
                    _logger.LogInformation("User {UserId} ({Email}) has login: Provider={Provider}, Key={Key}",
                        user.Id, user.Email, login.LoginProvider, login.ProviderKey);

                    // Compare keys case-insensitively to avoid potential casing issues
                    if (string.Equals(login.ProviderKey, nameIdentifier, StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogInformation("User found with external login: {UserId}", user.Id);
                        return user;
                    }
                }
            }

            _logger.LogWarning("No user found with external login name identifier: {NameIdentifier}", nameIdentifier);
            return null;
        }
    }
}
