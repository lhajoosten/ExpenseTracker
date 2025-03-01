using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace ExpenseTracker.Infrastructure.OAuth.Providers
{
    /// <summary>
    /// Implementation of OAuth provider functionality specific to Microsoft authentication
    /// </summary>
    public class MicrosoftOAuthProvider(ILogger<MicrosoftOAuthProvider> logger) : BaseOAuthProvider(logger)
    {
        private const string PLACEHOLDER = "";

        public override string ProviderName => "Microsoft";

        public override bool IsFromProvider(ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                return false;
            }

            // Microsoft typically includes this claim
            if (principal.HasClaim(c => c.Type == "http://schemas.microsoft.com/identity/claims/identityprovider"))
            {
                return true;
            }

            // Secondary check: look for Microsoft-specific claims or patterns
            var issuer = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(issuer) &&
                (issuer.Contains("microsoftonline") || issuer.Contains("live.com")))
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

            // Microsoft provides separate GivenName and Surname claims
            string firstName = principal.FindFirstValue(ClaimTypes.GivenName) ?? PLACEHOLDER;
            string lastName = principal.FindFirstValue(ClaimTypes.Surname) ?? PLACEHOLDER;

            // If GivenName/Surname not found, try to extract from the Name claim
            if (string.IsNullOrEmpty(firstName) && string.IsNullOrEmpty(lastName))
            {
                var name = principal.FindFirstValue(ClaimTypes.Name);
                if (!string.IsNullOrEmpty(name) && name.Contains(" "))
                {
                    var parts = name.Split(' ', 2);
                    firstName = parts[0];
                    lastName = parts.Length > 1 ? parts[1] : PLACEHOLDER;
                }
                else if (!string.IsNullOrEmpty(name))
                {
                    firstName = name;
                }
            }

            _logger.LogInformation("Extracted name from Microsoft claims - FirstName: {FirstName}, LastName: {LastName}",
                firstName, lastName);

            return (firstName, lastName);
        }

        public override string GetDisplayName(ClaimsPrincipal principal, string fallbackEmail)
        {
            // Try to get a display name from Microsoft claims
            var displayName = principal.FindFirstValue(ClaimTypes.Name);

            if (string.IsNullOrEmpty(displayName))
            {
                var firstName = principal.FindFirstValue(ClaimTypes.GivenName);
                var lastName = principal.FindFirstValue(ClaimTypes.Surname);

                if (!string.IsNullOrEmpty(firstName) || !string.IsNullOrEmpty(lastName))
                {
                    displayName = $"{firstName} {lastName}".Trim();
                }
            }

            return !string.IsNullOrEmpty(displayName) ? displayName : fallbackEmail;
        }
    }
}