using ExpenseTracker.Common.Abstractions.OAuth;
using ExpenseTracker.Infrastructure.OAuth.Providers;
using ExpenseTracker.Infrastructure.OAuth.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ExpenseTracker.Infrastructure.OAuth.Configuration
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddOAuthServices(this IServiceCollection services)
        {
            // Register providers
            services.AddScoped<MicrosoftOAuthProvider>();
            services.AddScoped<GitHubOAuthProvider>();

            // Register services
            services.AddScoped<IOAuthProviderService, OAuthProviderFactory>();
            services.AddScoped<IOAuthUserService, OAuthUserService>();
            services.AddScoped<IOAuthService, OAuthService>();

            return services;
        }
    }
}
