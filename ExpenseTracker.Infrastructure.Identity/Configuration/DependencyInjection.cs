using AutoMapper;
using ExpenseTracker.Common.Abstractions;
using ExpenseTracker.Common.Abstractions.Identity;
using ExpenseTracker.Infrastructure.Identity.Configuration;
using ExpenseTracker.Infrastructure.Identity.Data;
using ExpenseTracker.Infrastructure.Identity.Mapping;
using ExpenseTracker.Infrastructure.Identity.Models;
using ExpenseTracker.Infrastructure.Identity.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace ExpenseTracker.Infrastructure.Identity.Configuration
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddIdentityService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddIdenityContext(configuration);
            services.AddIdentity(configuration);
            services.AddAuthentication(configuration);
            services.AddAuthServices(configuration);
            services.AddIdentityMapping();

            return services;
        }

        private static void AddIdenityContext(this IServiceCollection services, IConfiguration configuration)
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
        }

        private static void AddIdentity(this IServiceCollection services, IConfiguration configuration)
        {
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
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddRoles<AppRole>()
            .AddRoleManager<RoleManager<AppRole>>()
            .AddUserManager<UserManager<AppUser>>()
            .AddSignInManager<SignInManager<AppUser>>()
            .AddEntityFrameworkStores<ETIdentityContext>()
            .AddDefaultTokenProviders();
        }

        private static void AddAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpContextAccessor();
            services.AddSingleton<ISecureDataFormat<AuthenticationProperties>, SessionOAuthStateStore>();

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "Identity.Application";
                options.DefaultChallengeScheme = "Identity.Application";
                options.DefaultSignInScheme = "Identity.Application";
            })
            .AddCookie("Identity.Application", options =>
            {
                options.Cookie.Name = "Identity.Application";
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

                options.ExpireTimeSpan = TimeSpan.FromHours(4); // Shorter, more controlled session
                options.SlidingExpiration = false; // Prevent automatic extension     
                options.Cookie.IsEssential = true; // Add more robust cookie validation

                // Validate the principal when reading the cookie
                options.Events.OnValidatePrincipal = async context =>
                {
                    // Check for required claims
                    var nameClaim = context.Principal!.FindFirstValue(ClaimTypes.Name);
                    var emailClaim = context.Principal!.FindFirstValue(ClaimTypes.Email);
                    var idClaim = context.Principal!.FindFirstValue(ClaimTypes.NameIdentifier);

                    if (string.IsNullOrEmpty(idClaim) || (string.IsNullOrEmpty(nameClaim) && string.IsNullOrEmpty(emailClaim)))
                    {
                        context.RejectPrincipal();
                        await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    }
                };

                // Add logging for diagnosis
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
                    },
                    OnSigningIn = context =>
                    {
                        Console.WriteLine("Signing in user");
                        return Task.CompletedTask;
                    },
                    OnSignedIn = context =>
                    {
                        Console.WriteLine("User signed in successfully");
                        return Task.CompletedTask;
                    }
                };
            })
            .AddMicrosoftAccount("Microsoft", options =>
            {
                options.ClientId = configuration["Authentication:Microsoft:ClientId"]!;
                options.ClientSecret = configuration["Authentication:Microsoft:ClientSecret"]!;
                options.CallbackPath = configuration["Authentication:Microsoft:CallbackPath"]!;
                options.Scope.Add("User.Read");
                options.SaveTokens = true;

                // Force correlation cookie creation
                options.CorrelationCookie.Name = ".AspNetCore.Correlation.Microsoft";
                options.CorrelationCookie.HttpOnly = true;
                options.CorrelationCookie.SameSite = SameSiteMode.None;
                options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
                options.CorrelationCookie.Expiration = TimeSpan.FromMinutes(30);

                options.SignInScheme = "Identity.Application";

                // Add event handlers
                options.Events.OnRemoteFailure = context => {
                    if (context.Failure != null &&
                        (context.Failure.Message.Contains("state") || context.Failure.Message.Contains("correlation")))
                    {
                        // If the user is already authenticated, redirect to success
                        if (context.HttpContext.User.Identity?.IsAuthenticated == true)
                        {
                            context.Response.Redirect($"{configuration["App:ClientUrl"]}?auth=success");
                            context.HandleResponse();
                            return Task.CompletedTask;
                        }

                        // Otherwise handle the error
                        context.Response.Redirect($"/api/oauth/handle-state-failure?provider=Microsoft&error={Uri.EscapeDataString(context.Failure.Message)}");
                        context.HandleResponse();
                    }
                    return Task.CompletedTask;
                };
            })
            .AddGitHub("GitHub", options => // Explicitly name the scheme
            {
                options.ClientId = configuration["Authentication:GitHub:ClientId"]!;
                options.ClientSecret = configuration["Authentication:GitHub:ClientSecret"]!;
                options.CallbackPath = configuration["Authentication:GitHub:CallbackPath"]!;
                options.Scope.Add("user:email");
                options.SaveTokens = true;

                // Force correlation cookie creation
                options.CorrelationCookie.Name = ".AspNetCore.Correlation.GitHub";
                options.CorrelationCookie.HttpOnly = true;
                options.CorrelationCookie.SameSite = SameSiteMode.None;
                options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
                options.CorrelationCookie.Expiration = TimeSpan.FromMinutes(30);

                options.SignInScheme = "Identity.Application";

                // Add event handlers
                options.Events.OnRemoteFailure = context => {
                    if (context.Failure != null &&
                        (context.Failure.Message.Contains("state") || context.Failure.Message.Contains("correlation")))
                    {
                        // If the user is already authenticated, redirect to success
                        if (context.HttpContext.User.Identity?.IsAuthenticated == true)
                        {
                            context.Response.Redirect($"{configuration["App:ClientUrl"]}?auth=success");
                            context.HandleResponse();
                            return Task.CompletedTask;
                        }

                        // Otherwise handle the error
                        context.Response.Redirect($"/api/oauth/handle-state-failure?provider=GitHub&error={Uri.EscapeDataString(context.Failure.Message)}");
                        context.HandleResponse();
                    }
                    return Task.CompletedTask;
                };
            });
        }

        private static void AddAuthServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IUserClaimsPrincipalFactory<AppUser>, UserClaimsPrincipalFactory<AppUser, AppRole>>();
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
