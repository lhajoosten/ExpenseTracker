namespace ExpenseTracker.Common.Models
{
    public class ApplicationUserDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public ApplicationRoleDto Role { get; set; }
    }
}
