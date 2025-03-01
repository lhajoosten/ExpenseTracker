using ExpenseTracker.Common.Models.OAuth;
using System.Security.Claims;

namespace ExpenseTracker.Common.Abstractions.OAuth
{
    /// <summary>
    /// Service for handling user-related operations in the context of OAuth authentication
    /// </summary>
    public interface IOAuthUserService
    {
        /// <summary>
        /// Retrieves the current user's status and information for authenticated users
        /// </summary>
        Task<OAuthLoginResult> GetCurrentUserStatusAsync(ClaimsPrincipal principal);

        /// <summary>
        /// Ensures a user exists in the system based on their authentication claims
        /// </summary>
        Task<OAuthLoginResult> EnsureUserExistsAsync(ClaimsPrincipal principal);

        /// <summary>
        /// Retrieves user information from external login credentials
        /// </summary>
        Task<OAuthUserInfo?> GetUserFromExternalLoginAsync(string nameIdentifier, string provider);

        /// <summary>
        /// Creates or updates a user based on claims from an external provider
        /// </summary>
        Task<OAuthUserInfo?> CreateOrUpdateUserFromClaimsAsync(ClaimsPrincipal principal, string provider);

        /// <summary>
        /// Ensures a provider is linked to a user account
        /// </summary>
        Task EnsureProviderLinkedAsync(string userId, string nameIdentifier, string provider);

        /// <summary>
        /// Finds a user by their external login identifier
        /// </summary>
        Task<OAuthUserInfo?> FindUserByExternalLoginAsync(string nameIdentifier);
    }
}
