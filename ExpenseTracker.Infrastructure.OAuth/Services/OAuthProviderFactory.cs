using ExpenseTracker.Common.Abstractions.OAuth;
using ExpenseTracker.Infrastructure.OAuth.Providers;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace ExpenseTracker.Infrastructure.OAuth.Services
{
    /// <summary>
    /// Factory for creating and managing OAuth provider instances
    /// </summary>
    public class OAuthProviderFactory : IOAuthProviderService
    {
        private readonly ILogger<OAuthProviderFactory> _logger;
        private readonly Dictionary<string, BaseOAuthProvider> _providers;

        public OAuthProviderFactory(
            MicrosoftOAuthProvider microsoftProvider,
            GitHubOAuthProvider githubProvider,
            ILogger<OAuthProviderFactory> logger)
        {
            _logger = logger;

            // Register available providers
            _providers = new Dictionary<string, BaseOAuthProvider>(StringComparer.OrdinalIgnoreCase)
            {
                { microsoftProvider.ProviderName, microsoftProvider },
                { githubProvider.ProviderName, githubProvider }
            };
        }

        public string NormalizeProviderName(string provider)
        {
            return provider?.ToLower() switch
            {
                "microsoft" => "Microsoft",
                "github" => "GitHub",
                _ => throw new ArgumentException($"Unsupported provider: {provider}")
            };
        }

        public bool IsProviderSupported(string provider)
        {
            try
            {
                var normalizedName = NormalizeProviderName(provider);
                return _providers.ContainsKey(normalizedName);
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        public string DetermineProvider(ClaimsPrincipal principal)
        {
            ArgumentNullException.ThrowIfNull(principal);

            _logger.LogInformation("Determining provider from claims principal");

            // Log all available claims for debugging
            _logger.LogDebug("Available claims:");
            foreach (var claim in principal.Claims)
            {
                _logger.LogDebug("  {Type}: {Value}", claim.Type, claim.Value);
            }

            // Check for GitHub-specific claims
            if (principal.HasClaim(c => c.Type == "urn:github:login") ||
                principal.HasClaim(c => c.Type == "urn:github:name") ||
                principal.HasClaim(c => c.Type == "urn:github:url") ||
                principal.FindFirstValue(ClaimTypes.NameIdentifier)?.Contains("github", StringComparison.OrdinalIgnoreCase) == true)
            {
                _logger.LogInformation("Provider determined to be: GitHub");
                return "GitHub";
            }

            // Check for Microsoft-specific claims
            if (principal.HasClaim(c => c.Type == "http://schemas.microsoft.com/identity/claims/tenantid") ||
                principal.HasClaim(c => c.Type == "http://schemas.microsoft.com/identity/claims/identityprovider") ||
                principal.FindFirstValue(ClaimTypes.NameIdentifier)?.Contains("live.com", StringComparison.OrdinalIgnoreCase) == true ||
                principal.FindFirstValue(ClaimTypes.NameIdentifier)?.Contains("microsoftonline", StringComparison.OrdinalIgnoreCase) == true)
            {
                _logger.LogInformation("Provider determined to be: Microsoft");
                return "Microsoft";
            }

            // If unable to determine, default to Microsoft but log a warning
            _logger.LogWarning("Could not determine provider from claims, defaulting to Microsoft");
            return "Microsoft";
        }

        public (string firstName, string lastName) ExtractNameFromClaims(ClaimsPrincipal principal, string provider)
        {
            ArgumentNullException.ThrowIfNull(principal);

            if (string.IsNullOrEmpty(provider) || !_providers.TryGetValue(NormalizeProviderName(provider), out var oauthProvider))
            {
                _logger.LogWarning("Provider {Provider} not found, using generic name extraction", provider);
                return ExtractNameGeneric(principal);
            }

            return oauthProvider.ExtractNameFromClaims(principal);
        }

        // Generic fallback for unknown providers
        private static (string firstName, string lastName) ExtractNameGeneric(ClaimsPrincipal principal)
        {
            string firstName = principal.FindFirstValue(ClaimTypes.GivenName) ??
                              principal.FindFirstValue(ClaimTypes.Name)?.Split(' ').FirstOrDefault() ?? "";

            string lastName = principal.FindFirstValue(ClaimTypes.Surname) ??
                             (principal.FindFirstValue(ClaimTypes.Name)?.Split(' ').Skip(1).FirstOrDefault() ?? "");

            return (firstName, lastName);
        }
    }
}