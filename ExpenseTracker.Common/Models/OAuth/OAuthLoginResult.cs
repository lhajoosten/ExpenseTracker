namespace ExpenseTracker.Common.Models.OAuth
{
    /// <summary>
    /// Represents the result of an OAuth login operation
    /// </summary>
    public class OAuthLoginResult
    {
        /// <summary>
        /// Indicates whether the authentication was successful
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// The URL to redirect to after authentication
        /// </summary>
        public string RedirectUrl { get; set; } = string.Empty;

        /// <summary>
        /// User information if authentication was successful
        /// </summary>
        public ApplicationUserDto? User { get; set; }

        /// <summary>
        /// Error message if authentication failed
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Additional details about the authentication result
        /// </summary>
        public string? Details { get; set; }

        /// <summary>
        /// Creates a successful authentication result
        /// </summary>
        public static OAuthLoginResult Success(string redirectUrl, ApplicationUserDto user)
        {
            return new OAuthLoginResult
            {
                IsSuccess = true,
                RedirectUrl = redirectUrl,
                User = user
            };
        }

        /// <summary>
        /// Creates a failed authentication result
        /// </summary>
        public static OAuthLoginResult Failure(string redirectUrl, string errorMessage, string? details = null)
        {
            return new OAuthLoginResult
            {
                IsSuccess = false,
                RedirectUrl = redirectUrl,
                ErrorMessage = errorMessage,
                Details = details
            };
        }
    }
}