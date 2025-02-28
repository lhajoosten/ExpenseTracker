// Create in Infrastructure/Identity/Services/SessionOAuthStateStore.cs
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace ExpenseTracker.Infrastructure.Identity.Services
{
    public class SessionOAuthStateStore : ISecureDataFormat<AuthenticationProperties>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string StateKeyPrefix = "OAuthState:";

        public SessionOAuthStateStore(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string Protect(AuthenticationProperties data)
        {
            return Protect(data, null);
        }

        public string Protect(AuthenticationProperties data, string purpose)
        {
            // Generate a unique ID for this state
            string stateId = Guid.NewGuid().ToString();

            // Store the state in session
            string serialized = JsonSerializer.Serialize(data.Items);
            _httpContextAccessor.HttpContext.Session.SetString(StateKeyPrefix + stateId, serialized);

            return stateId;
        }

        public AuthenticationProperties Unprotect(string protectedText)
        {
            return Unprotect(protectedText, null);
        }

        public AuthenticationProperties Unprotect(string protectedText, string purpose)
        {
            if (string.IsNullOrEmpty(protectedText))
            {
                return null;
            }

            // Try to get the state from session
            string key = StateKeyPrefix + protectedText;
            string serialized = _httpContextAccessor.HttpContext.Session.GetString(key);

            if (string.IsNullOrEmpty(serialized))
            {
                return null;
            }

            // Remove it from session (one-time use)
            _httpContextAccessor.HttpContext.Session.Remove(key);

            // Deserialize and return
            var items = JsonSerializer.Deserialize<Dictionary<string, string>>(serialized);
            return new AuthenticationProperties(items);
        }
    }
}