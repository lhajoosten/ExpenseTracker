﻿namespace ExpenseTracker.Common.Models.Identity
{
    public class TokenResponse
    {
        public Guid UserId { get; set; }
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime Expires { get; set; }
    }
}
