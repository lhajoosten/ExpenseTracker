using AutoMapper;
using ExpenseTracker.Common.Abstractions;
using ExpenseTracker.Infrastructure.Identity.Data;
using ExpenseTracker.Infrastructure.Identity.JwtSettings;
using ExpenseTracker.Infrastructure.Identity.Mapping;
using ExpenseTracker.Infrastructure.Identity.Models;
using ExpenseTracker.Infrastructure.Identity.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ExpenseTracker.Infrastructure.Identity
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddIdentityService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ETIdentityContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DevelopmentConnection"),
                    b => b.MigrationsAssembly(typeof(ETIdentityContext).Assembly.FullName)));

            services.AddIdentityServices(configuration);
            services.AddAuthServices(configuration);
            services.AddIdentityMapping();

            return services;
        }

        private static void AddIdentityServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register JWT Settings
            services.Configure<JwtSetting>(configuration.GetSection("JwtSettings"));

            var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSetting>();
            var key = Encoding.UTF8.GetBytes(jwtSettings!.Secret);

            // Register JWT Authentication
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
            }).AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    ClockSkew = TimeSpan.Zero
                };
            });

            services.AddIdentityCore<AppUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.SignIn.RequireConfirmedPhoneNumber = false;
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
