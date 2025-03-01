namespace ExpenseTracker.Common.Models.OAuth
{
    /// <summary>
    /// Contains essential user information extracted from OAuth authentication
    /// </summary>
    public class OAuthUserInfo
    {
        /// <summary>
        /// The unique identifier from the OAuth provider
        /// </summary>
        public string NameIdentifier { get; set; } = string.Empty;

        /// <summary>
        /// The user's email address
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// The user's first name
        /// </summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// The user's last name
        /// </summary>
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// The display name (could be username or full name)
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// The OAuth provider name (e.g., "Microsoft", "GitHub")
        /// </summary>
        public string Provider { get; set; } = string.Empty;
    }
}
