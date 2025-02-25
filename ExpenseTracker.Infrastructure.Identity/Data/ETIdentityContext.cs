using ExpenseTracker.Infrastructure.Identity.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastructure.Identity.Data
{
    public class ETIdentityContext : IdentityDbContext<AppUser, AppRole, Guid>
    {
        public ETIdentityContext(DbContextOptions<ETIdentityContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure the foreign key for RoleId in AppUser
            builder.Entity<AppUser>()
                .HasOne(u => u.Role)
                .WithMany()
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
