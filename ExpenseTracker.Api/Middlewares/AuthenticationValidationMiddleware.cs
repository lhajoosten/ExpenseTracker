using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace ExpenseTracker.Api.Middlewares
{
    public class AuthenticationValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuthenticationValidationMiddleware> _logger;

        public AuthenticationValidationMiddleware(RequestDelegate next, ILogger<AuthenticationValidationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                // Validate that the authenticated user has the minimum required claims
                if (string.IsNullOrEmpty(context.User.FindFirstValue(ClaimTypes.NameIdentifier)) ||
                    string.IsNullOrEmpty(context.User.FindFirstValue(ClaimTypes.Email)))
                {
                    _logger.LogWarning("Invalid authentication detected - required claims missing");

                    // Clear auth and continue as unauthenticated
                    await context.SignOutAsync();
                    context.User = new ClaimsPrincipal(new ClaimsIdentity());
                }
            }

            await _next(context);
        }
    }

    // Extension method
    public static class AuthenticationValidationMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuthenticationValidation(this IApplicationBuilder app)
        {
            return app.UseMiddleware<AuthenticationValidationMiddleware>();
        }
    }
}
