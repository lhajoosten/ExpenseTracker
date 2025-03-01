using Microsoft.AspNetCore.Authentication;

namespace ExpenseTracker.Api.Middlewares
{
    public class AuthenticationExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuthenticationExceptionHandlerMiddleware> _logger;
        private readonly IConfiguration _configuration;

        public AuthenticationExceptionHandlerMiddleware(
            RequestDelegate next,
            ILogger<AuthenticationExceptionHandlerMiddleware> logger,
            IConfiguration configuration)
        {
            _next = next;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex) when (IsAuthenticationStateError(ex))
            {
                _logger.LogWarning(ex, "Authentication state validation error intercepted");

                // If user is already authenticated, we can simply redirect to success
                if (context.User.Identity?.IsAuthenticated == true)
                {
                    _logger.LogInformation("User is already authenticated, redirecting to dashboard");
                    context.Response.Redirect($"{_configuration["App:ClientUrl"]}/dashboard?auth=success");
                    return;
                }

                // Otherwise, redirect to login page with error message
                context.Response.Redirect($"{_configuration["App:ClientUrl"]}?error=authentication_failed&message={Uri.EscapeDataString("Authentication state validation failed")}");
            }
        }

        private bool IsAuthenticationStateError(Exception ex)
        {
            return (ex is AuthenticationFailureException authEx &&
                   (authEx.Message.Contains("state") || authEx.Message.Contains("correlation"))) ||
                   (ex.InnerException is AuthenticationFailureException innerAuthEx &&
                   (innerAuthEx.Message.Contains("state") || innerAuthEx.Message.Contains("correlation")));
        }
    }

    // Extension method for easier middleware registration
    public static class AuthenticationExceptionHandlerMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuthenticationExceptionHandler(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthenticationExceptionHandlerMiddleware>();
        }
    }
}
