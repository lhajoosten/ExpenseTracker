using ExpenseTracker.Common.Models;

namespace ExpenseTracker.Common.Abstractions
{
    public interface IEmailService
    {
        Task<Result> SendEmailConfirmationAsync(string email, string token);

        Task<Result> SendPasswordResetAsync(string email, string token);
    }
}
