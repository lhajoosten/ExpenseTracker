namespace ExpenseTracker.Common.Models.Identity
{
    public class EmailChangeRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string NewEmail { get; set; } = string.Empty;
    }
}
