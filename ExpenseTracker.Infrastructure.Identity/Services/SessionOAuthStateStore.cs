using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;

namespace ExpenseTracker.Infrastructure.Identity.Services
{
    public class SessionOAuthStateStore(
        IDataProtectionProvider dataProtectionProvider) : ISecureDataFormat<AuthenticationProperties>
    {
        private readonly PropertiesDataFormat _propertiesDataFormat = new PropertiesDataFormat(
                dataProtectionProvider.CreateProtector(
                    "Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationMiddleware",
                    "Identity.Application",
                    "v2"));

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
                return new AuthenticationProperties();
            }
        }
    }
}