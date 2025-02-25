namespace ExpenseTracker.Infrastructure.Identity.JwtSettings
{
    public class JwtSetting
    {
        public string Secret { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int ExtendedExpirationInMinutes { get; set; }
        public int StandardExpirationInMinutes { get; set; }
    }
}
