namespace ExpenseTracker.Common.Models
{
    public class SignInRequest(string email, string password, bool rememberMe)
    {
        public string Email { get; set; } = email;
        public string Password { get; set; } = password;
        public bool RememberMe { get; set; } = rememberMe;
    }
}
