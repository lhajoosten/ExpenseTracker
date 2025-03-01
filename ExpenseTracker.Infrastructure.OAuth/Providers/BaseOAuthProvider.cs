using ExpenseTracker.Common.Models.OAuth;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace ExpenseTracker.Infrastructure.OAuth.Providers
{
    /// <summary>
    /// Base implementation for provider-specific OAuth functionality
    /// </summary>
    public abstract class BaseOAuthProvider(ILogger<BaseOAuthProvider> logger)
    {
        protected readonly ILogger<BaseOAuthProvider> _logger = logger;

        /// <summary>
        /// Gets the provider name
        /// </summary>
        public abstract string ProviderName { get; }

        /// <summary>
        /// Determines if a ClaimsPrincipal originated from this provider
        /// </summary>
        public abstract bool IsFromProvider(ClaimsPrincipal principal);

        /// <summary>
        /// Extracts user information from claims
        /// </summary>
        public abstract (string firstName, string lastName) ExtractNameFromClaims(ClaimsPrincipal principal);

        /// <summary>
        /// Gets the display name claim for this provider
        /// </summary>
        public abstract string GetDisplayName(ClaimsPrincipal principal, string fallbackEmail);

        /// <summary>
        /// Creates a standardized user info object from claims
        /// </summary>
        public OAuthUserInfo CreateUserInfo(ClaimsPrincipal principal)
        {
            var nameIdentifier = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = principal.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("No email claim found for {Provider}", ProviderName);
                return null!;
            }

            var (firstName, lastName) = ExtractNameFromClaims(principal);
            var displayName = GetDisplayName(principal, email);

            return new OAuthUserInfo
            {
                NameIdentifier = nameIdentifier ?? "",
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                DisplayName = displayName,
                Provider = ProviderName
            };
        }
    }
}
