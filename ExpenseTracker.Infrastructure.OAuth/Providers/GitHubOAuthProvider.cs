using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace ExpenseTracker.Infrastructure.OAuth.Providers
{
    /// <summary>
    /// Implementation of OAuth provider functionality specific to GitHub authentication
    /// </summary>
    public class GitHubOAuthProvider(ILogger<GitHubOAuthProvider> logger) : BaseOAuthProvider(logger)
    {
        private const string PLACEHOLDER = "";

        public override string ProviderName => "GitHub";

        public override bool IsFromProvider(ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                return false;
            }

            // GitHub includes these claims
            if (principal.HasClaim(c => c.Type == "urn:github:login") ||
                principal.HasClaim(c => c.Type == "urn:github:name") ||
                principal.HasClaim(c => c.Type == "urn:github:url"))
            {
                return true;
            }

            // Secondary check: look for GitHub patterns in the identifier
            var nameIdentifier = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(nameIdentifier) && nameIdentifier.Contains("github", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        public override (string firstName, string lastName) ExtractNameFromClaims(ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                return (PLACEHOLDER, PLACEHOLDER);
            }

            // For GitHub, prioritize the full name (urn:github:name) over the username (ClaimTypes.Name)
            var fullName = principal.FindFirstValue("urn:github:name") ??
                          principal.FindFirstValue(ClaimTypes.Name) ?? PLACEHOLDER;

            string firstName = PLACEHOLDER, lastName = PLACEHOLDER;

            _logger.LogInformation("GitHub full name: {FullName}", fullName);

            if (!string.IsNullOrEmpty(fullName) && fullName.Contains(" "))
            {
                // Split full name into first and last name
                var parts = fullName.Split(' ', 2);
                firstName = parts[0];
                lastName = parts.Length > 1 ? parts[1] : PLACEHOLDER;
            }
            else
            {
                // If no space in name, use as first name (likely a username)
                // This could be either the display name or the GitHub username
                firstName = fullName;

                // Try getting the GitHub username separately
                var githubLogin = principal.FindFirstValue("urn:github:login");
                if (!string.IsNullOrEmpty(githubLogin) && string.IsNullOrEmpty(firstName))
                {
                    firstName = githubLogin;
                }
            }

            _logger.LogInformation("Extracted name from GitHub claims - FirstName: {FirstName}, LastName: {LastName}",
                firstName, lastName);

            return (firstName, lastName);
        }

        public override string GetDisplayName(ClaimsPrincipal principal, string fallbackEmail)
        {
            // For GitHub, prefer the name claim if available
            var displayName = principal.FindFirstValue("urn:github:name") ??
                             principal.FindFirstValue(ClaimTypes.Name);

            // If no name, try to get the GitHub username
            if (string.IsNullOrEmpty(displayName))
            {
                displayName = principal.FindFirstValue("urn:github:login");
            }

            return !string.IsNullOrEmpty(displayName) ? displayName : fallbackEmail;
        }
    }
}