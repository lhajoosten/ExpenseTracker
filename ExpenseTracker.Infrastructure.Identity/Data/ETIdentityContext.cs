using ExpenseTracker.Infrastructure.Identity.Models;
using Microsoft.AspNetCore.Identity;
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

            builder.HasDefaultSchema("identity");

            builder.Entity<AppRole>(entity => { entity.ToTable(name: "Role"); });
            builder.Entity<AppUser>(entity => { entity.ToTable(name: "User"); });
            builder.Entity<IdentityUserRole<Guid>>(entity => { entity.ToTable("UserRole"); });
            builder.Entity<IdentityUserLogin<Guid>>(entity => { entity.ToTable("UserLogin"); });

            // Ignoring the following tables
            builder.Ignore<IdentityUserClaim<Guid>>();
            builder.Ignore<IdentityRoleClaim<Guid>>();
            builder.Ignore<IdentityUserToken<Guid>>();
        }
    }
}
