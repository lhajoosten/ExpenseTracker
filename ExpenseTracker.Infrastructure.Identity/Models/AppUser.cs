using Microsoft.AspNetCore.Identity;

namespace ExpenseTracker.Infrastructure.Identity.Models
{
    public class AppUser : IdentityUser<Guid>
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? FullName => $"{FirstName} {LastName}";
        public DateTime? Birthdate { get; set; }
    }
}
