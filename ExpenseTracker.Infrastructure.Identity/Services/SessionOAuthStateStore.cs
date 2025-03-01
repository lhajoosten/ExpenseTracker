// Create in Infrastructure/Identity/Services/SessionOAuthStateStore.cs
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;

namespace ExpenseTracker.Infrastructure.Identity.Services
{
    public class SessionOAuthStateStore : ISecureDataFormat<AuthenticationProperties>
    {
        private readonly IDataProtector _dataProtector;
        private readonly PropertiesDataFormat _propertiesDataFormat;

        public SessionOAuthStateStore(
            IDataProtectionProvider dataProtectionProvider,
            IOptions<CookieAuthenticationOptions> cookieOptions)
        {
            _dataProtector = dataProtectionProvider.CreateProtector("Microsoft.AspNetCore.Authentication.OAuth");
            _propertiesDataFormat = new PropertiesDataFormat(
                dataProtectionProvider.CreateProtector(
                    "Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationMiddleware",
                    "Identity.Application",
                    "v2"));
        }

        public string Protect(AuthenticationProperties data)
        {
            return Protect(data, null);
        }

        public string Protect(AuthenticationProperties data, string? purpose)
        {
            if (data == null)
            {
                return null!;
            }

            return _propertiesDataFormat.Protect(data);
        }

        public AuthenticationProperties? Unprotect(string? protectedText)
        {
            return Unprotect(protectedText, null);
        }

        public AuthenticationProperties? Unprotect(string? protectedText, string? purpose)
        {
            try
            {
                if (string.IsNullOrEmpty(protectedText))
                {
                    return null;
                }

                return _propertiesDataFormat.Unprotect(protectedText);
            }
            catch
            {
                // If anything fails, just return a fresh AuthenticationProperties
                return new AuthenticationProperties();
            }
        }
    }
}