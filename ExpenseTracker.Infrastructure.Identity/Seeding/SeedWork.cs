using ExpenseTracker.Infrastructure.Identity.Data;
using ExpenseTracker.Infrastructure.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ExpenseTracker.Infrastructure.Identity.Seeding
{
    public static class SeedWork
    {
        public static async Task SeedIdentityDataAsync(this IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ETIdentityContext>();
            await context.Database.MigrateAsync();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

            await SeedRolesAsync(roleManager);
            await SeedAdminAccountAsync(userManager, roleManager);
        }

        private static async Task SeedRolesAsync(RoleManager<AppRole> roleManager)
        {
            // Define your default roles here
            var roles = new List<string> { "User", "Admin", "Financial Manager", "Deactivated" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new AppRole { Name = role });
                }
            }
        }

        private static async Task SeedAdminAccountAsync(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
        {
            var adminRole = await roleManager.FindByNameAsync("Admin");
            if (adminRole == null)
                return;

            var adminEmail = "admin@expense-tracker.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new AppUser
                {
                    UserName = "ET-Admin",
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FirstName = "Admin",
                    LastName = "ET",
                    Birthdate = new DateTime(1999, 12, 14)
                };

                var createResult = await userManager.CreateAsync(adminUser, "Administrator1234!");
                if (!createResult.Succeeded)
                {
                    throw new Exception($"Unable to create admin user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                }
            }

            if (!await userManager.IsInRoleAsync(adminUser, adminRole.Name!))
            {
                var addRoleResult = await userManager.AddToRoleAsync(adminUser, adminRole.Name!);
                if (!addRoleResult.Succeeded)
                {
                    throw new Exception($"Unable to add user to admin role: {string.Join(", ", addRoleResult.Errors.Select(e => e.Description))}");
                }
            }
        }
    }
}
