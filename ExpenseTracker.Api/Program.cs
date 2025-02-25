using ExpenseTracker.Infrastructure.Identity;
using ExpenseTracker.Infrastructure.Mailing;
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

            var app = builder.Build();

            // Use the CORS policy
            app.UseCors(options =>
            {
                options.WithOrigins(["https://localhost:4443", "http://localhost:4200"]);
                options.AllowAnyMethod();
                options.AllowAnyHeader();
                options.AllowCredentials();
            });

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
            }

            // Configure middleware pipeline
            if (!app.Environment.IsDevelopment())
            {
                app.UseHsts();
            }

            // Use Https Redirection to enforce HTTPS
            app.UseHttpsRedirection();

            // Use Authentication & Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            // Map Api Controllers
            app.MapControllers();

            app.Run();
        }
    }
}
