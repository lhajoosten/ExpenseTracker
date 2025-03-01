using Azure.Identity;
using ExpenseTracker.Api.Middlewares;
using ExpenseTracker.Infrastructure.Identity.Configuration;
using ExpenseTracker.Infrastructure.Identity.Seeding;
using ExpenseTracker.Infrastructure.Mailing.Configuration;
using ExpenseTracker.Infrastructure.OAuth.Configuration;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.OpenApi.Models;

namespace ExpenseTracker.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Create the builder and retrieve configuration & services.
            var builder = WebApplication.CreateBuilder(args);
            var configuration = builder.Configuration;
            var services = builder.Services;

            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);

            // Configure Kestrel for HTTPS defaults.
            builder.WebHost.ConfigureKestrel(serverOptions =>
            {
                serverOptions.ConfigureHttpsDefaults(configureOptions =>
                {
                    configureOptions.SslProtocols = System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls13;
                });
            });

            // Configure Azure Key Vault for Production.
            if (builder.Environment.IsProduction())
            {
                string keyVaultUri = configuration["VaultUri"]!;
                if (!string.IsNullOrEmpty(keyVaultUri))
                {
                    var credential = new DefaultAzureCredential();
                    configuration.AddAzureKeyVault(new Uri(keyVaultUri), credential);
                }
            }

            // Service Registration.
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(60);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SameSite = SameSiteMode.None;
            });
            services.AddIdentityService(configuration);
            services.AddMailingService(configuration);
            services.AddOAuthServices();
            services.AddHttpContextAccessor();
            services.AddDataProtection();
            services.AddControllers();

            // Register Swagger with JWT Support.
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "ExpenseTracker API", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Enter 'Bearer {your token}'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
                // Set Swagger base path explicitly.
                c.AddServer(new OpenApiServer { Url = "https://localhost:8443" });
            });

            // Register CORS Policy.
            services.AddCors(options =>
            {
                options.AddPolicy("ClientCorsPolicy", policy =>
                {
                    policy.WithOrigins("https://localhost:4443")
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials(); // Critical for cookies.
                });
            });

            // Build the application.
            var app = builder.Build();

            // Middleware Pipeline Configuration.

            // 2. Seed Identity Data.
            app.Services.SeedIdentityDataAsync().GetAwaiter().GetResult();

            // 3. Environment-specific Middleware.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger(c =>
                {
                    c.RouteTemplate = "api/v1/swagger/{documentName}/swagger.json";
                });
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/api/v1/swagger/v1/swagger.json", "ExpenseTracker.Api v1");
                    c.RoutePrefix = "api/v1/swagger";
                });
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            // 4. Use Exception Handler Middleware.
            app.UseAuthenticationExceptionHandler();

            // 5. Enforce HTTPS.
            app.UseHttpsRedirection();

            // 6. Enable Routing.
            app.UseRouting();

            // 7. Use Session early.
            app.UseSession();

            // 8. Apply CORS Policy.
            app.UseCors("ClientCorsPolicy");

            // 9. Apply Authentication & Authorization.
            app.UseAuthentication();
            app.UseAuthorization();

            // 10. Map API Controllers.
            app.MapControllers();

            // Run the application.
            app.Run();
        }
    }
}
