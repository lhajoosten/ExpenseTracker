﻿namespace ExpenseTracker.Common.Models.Identity
{
    public class PasswordResetRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
