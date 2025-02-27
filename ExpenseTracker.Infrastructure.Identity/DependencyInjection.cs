using AutoMapper;
using ExpenseTracker.Common.Abstractions;
using ExpenseTracker.Infrastructure.Identity.Data;
using ExpenseTracker.Infrastructure.Identity.Mapping;
using ExpenseTracker.Infrastructure.Identity.Models;
using ExpenseTracker.Infrastructure.Identity.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ExpenseTracker.Infrastructure.Identity
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddIdentityService(this IServiceCollection services, IConfiguration configuration)
        {
            var env = configuration["ASPNETCORE_ENVIRONMENT"];
            if (env == "Development")
            {
                services.AddDbContext<ETIdentityContext>(options =>
                    options.UseSqlServer(
                        configuration.GetConnectionString("DevelopmentConnection"),
                        b => b.MigrationsAssembly(typeof(ETIdentityContext).Assembly.FullName)
                    )
                );
            }
            else
            {
                services.AddDbContext<ETIdentityContext>(options =>
                    options.UseSqlServer(
                        configuration.GetConnectionString("ProductionConnection"),
                        b => b.MigrationsAssembly(typeof(ETIdentityContext).Assembly.FullName)
                    )
                );
            }

            services.AddIdentity(configuration);
            services.AddAuthServices(configuration);
            services.AddIdentityMapping();

            return services;
        }

        private static void AddIdentity(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Identity.Application";
                options.DefaultSignInScheme = "Identity.Application";
                options.DefaultChallengeScheme = "Identity.Application";
            })
            .AddCookie("Identity.Application", options =>
            {
                options.Cookie.Name = "ExpenseTracker.Identity";
                options.Events = new CookieAuthenticationEvents
                {
                    OnRedirectToLogin = context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    },
                    OnRedirectToAccessDenied = context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        return Task.CompletedTask;
                    }
                };
            })
            .AddMicrosoftAccount(options =>
            {
                options.ClientId = configuration["Authentication:Microsoft:ClientId"]!;
                options.ClientSecret = configuration["Authentication:Microsoft:ClientSecret"]!;
                options.Scope.Add("User.Read");
                options.CallbackPath = "/api/oauth/login-callback";
            })
            .AddGitHub(options =>
            {
                options.ClientId = configuration["Authentication:GitHub:ClientId"]!;
                options.ClientSecret = configuration["Authentication:GitHub:ClientSecret"]!;
                options.CallbackPath = "/api/oauth/login-callback";
            });

            services.AddIdentityCore<AppUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultProvider;
                options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultProvider;
                options.Tokens.ChangeEmailTokenProvider = TokenOptions.DefaultProvider;
                options.Tokens.ChangePhoneNumberTokenProvider = TokenOptions.DefaultProvider;
                options.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultProvider;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.SignIn.RequireConfirmedPhoneNumber = false;
                options.SignIn.RequireConfirmedEmail = true;
            })
            .AddRoles<AppRole>()
            .AddRoleManager<RoleManager<AppRole>>()
            .AddUserManager<UserManager<AppUser>>()
            .AddSignInManager<SignInManager<AppUser>>()
            .AddEntityFrameworkStores<ETIdentityContext>()
            .AddTokenProvider<DataProtectorTokenProvider<AppUser>>(TokenOptions.DefaultProvider);
        }


        private static void AddAuthServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserService, UserService>();
        }

        private static void AddIdentityMapping(this IServiceCollection services)
        {
            MapperConfiguration mappingConfig = new(mc =>
            {
                mc.AddProfile(new IdentityProfile());
            });

            services.AddSingleton(mappingConfig.CreateMapper());
        }
    }
}
