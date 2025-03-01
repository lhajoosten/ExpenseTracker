using ExpenseTracker.Common.Abstractions;
using ExpenseTracker.Infrastructure.Mailing.Models;
using ExpenseTracker.Infrastructure.Mailing.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ExpenseTracker.Infrastructure.Mailing
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddMailingService(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
            services.AddTransient<IEmailService, EmailService>();

            return services;
        }
    }
}
