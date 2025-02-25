using ExpenseTracker.Common.Models;

namespace ExpenseTracker.Common.Abstractions
{
    public interface IEmailService
    {
        Task<Result> SendEmailAsync(string email, string subject, string htmlMessage);

        Task<Result> SendEmailConfirmationAsync(string email, string token);

        Task<Result> SendPasswordResetAsync(string email, string token);
    }
}
