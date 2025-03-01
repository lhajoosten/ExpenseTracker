using ExpenseTracker.Infrastructure.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastructure.Identity.Data
{
    public class ETIdentityContext(DbContextOptions<ETIdentityContext> options) : IdentityDbContext<AppUser, AppRole, Guid>(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.HasDefaultSchema("identity");

            builder.Entity<AppRole>(entity => { entity.ToTable(name: "Role"); });
            builder.Entity<AppUser>(entity => { entity.ToTable(name: "User"); });
            builder.Entity<IdentityUserRole<Guid>>(entity => { entity.ToTable("UserRole"); });
            builder.Entity<IdentityUserLogin<Guid>>(entity => { entity.ToTable("UserLogin"); });
            builder.Entity<IdentityUserClaim<Guid>>(entity => { entity.ToTable("UserClaim"); });
            builder.Entity<IdentityRoleClaim<Guid>>(entity => { entity.ToTable("RoleClaim"); });
            builder.Entity<IdentityUserToken<Guid>>(entity => { entity.ToTable("UserToken"); });
        }
    }
}
