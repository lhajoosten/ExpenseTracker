using ExpenseTracker.Common.Models.OAuth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ExpenseTracker.Common.Abstractions.OAuth
{
    /// <summary>
    /// Primary service for coordinating OAuth-based authentication workflows
    /// </summary>
    public interface IOAuthService
    {
        /// <summary>
        /// Configures authentication properties for an external provider
        /// </summary>
        Task<AuthenticationProperties> ConfigureExternalAuthenticationAsync(string provider, string returnUrl);

        /// <summary>
        /// Processes the callback from an OAuth provider
        /// </summary>
        Task<OAuthLoginResult?> ProcessCallbackAsync(string provider, HttpContext httpContext, string clientUrl);

        /// <summary>
        /// Handles a callback that arrived without expected code and state parameters
        /// </summary>
        Task<OAuthLoginResult> HandleCallbackWithoutParamsAsync(ClaimsPrincipal user, string provider, string clientUrl);

        /// <summary>
        /// Handles state validation failures during the OAuth process
        /// </summary>
        Task<OAuthLoginResult> HandleStateFailureAsync(ClaimsPrincipal principal, string provider, string error);

        /// <summary>
        /// Handles state or correlation errors in the OAuth callback
        /// </summary>
        Task<OAuthLoginResult> HandleStateErrorAsync(ClaimsPrincipal principal, string provider, string error, string clientUrl);
    }
}
