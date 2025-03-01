using System.Security.Claims;

namespace ExpenseTracker.Common.Abstractions.OAuth
{
    /// <summary>
    /// Service for provider-specific OAuth functionality
    /// </summary>
    public interface IOAuthProviderService
    {
        /// <summary>
        /// Normalizes a provider name to a standard format
        /// </summary>
        string NormalizeProviderName(string provider);

        /// <summary>
        /// Checks if a provider is supported by the application
        /// </summary>
        bool IsProviderSupported(string provider);

        /// <summary>
        /// Determines the provider from a claims principal
        /// </summary>
        string DetermineProvider(ClaimsPrincipal principal);

        /// <summary>
        /// Extracts name information from claims based on provider
        /// </summary>
        (string firstName, string lastName) ExtractNameFromClaims(ClaimsPrincipal principal, string provider);
    }
}