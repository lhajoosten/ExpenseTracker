namespace ExpenseTracker.Common.Models
{
    public class ApplicationUserDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool EmailConfirmed { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public bool PhoneNumberConfirmed { get; set; }
        public IReadOnlyCollection<ApplicationRoleDto> Roles { get; set; } = [];
    }
}
