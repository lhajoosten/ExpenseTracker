using Azure.Identity;
using ExpenseTracker.Infrastructure.Identity;
using ExpenseTracker.Infrastructure.Identity.Seeding;
using ExpenseTracker.Infrastructure.Mailing;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.OpenApi.Models;

namespace ExpenseTracker.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var configuration = builder.Configuration;
            var services = builder.Services;

            builder.WebHost.ConfigureKestrel(serverOptions =>
            {
                serverOptions.ConfigureHttpsDefaults(configureOptions =>
                {
                    configureOptions.SslProtocols = System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls13;
                });
            });

            if (builder.Environment.IsProduction())
            {
                string keyVaultUri = builder.Configuration["VaultUri"]!;
                if (!string.IsNullOrEmpty(keyVaultUri))
                {
                    var credential = new DefaultAzureCredential();
                    builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUri), credential);
                }
            }

            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromDays(7);
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.None;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.IsEssential = true;
            });

            // Register Identity
            services.AddIdentityService(configuration);

            // Register Mailing
            services.AddMailingService(configuration);

            // Register IHttpContextAccessor
            services.AddHttpContextAccessor();

            // Register Data Protection
            services.AddDataProtection();

            // Add services to the container.
            builder.Services.AddControllers();

            // Register Swagger with JWT Support
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

                // Explicitly set Swagger base path
                c.AddServer(new OpenApiServer { Url = "https://localhost:8443" });
            });

            services.AddCors(options =>
            {
                options.AddPolicy("ClientCorsPolicy", policy =>
                {
                    policy.WithOrigins("https://localhost:4443")
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials(); // This is critical for cookies
                });
            });

            var app = builder.Build();

            // Use custom exception middleware
            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                    var exception = exceptionHandlerPathFeature?.Error;
                    // Log the exception
                    await context.Response.WriteAsync($"An error occurred see the error: ${exception?.Message}");
                });
            });


            app.Services.SeedIdentityDataAsync().GetAwaiter().GetResult();

            // Configure the HTTP request pipeline.
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

            app.UseSession();

            app.UseCookiePolicy(new CookiePolicyOptions
            {
                MinimumSameSitePolicy = SameSiteMode.Lax,
                Secure = CookieSecurePolicy.Always,
                HttpOnly = HttpOnlyPolicy.Always
            });

            // Use Https Redirection to enforce HTTPS
            app.UseHttpsRedirection();

            // Use Routing
            app.UseRouting();

            // Use the CORS policy
            app.UseCors("ClientCorsPolicy");

            // Use Authentication & Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            // Map Api Controllers
            app.MapControllers();

            app.Run();
        }
    }
}
