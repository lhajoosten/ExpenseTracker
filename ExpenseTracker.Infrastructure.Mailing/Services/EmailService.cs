using ExpenseTracker.Common.Abstractions;
using ExpenseTracker.Common.Models;
using ExpenseTracker.Infrastructure.Mailing.Models;
using ExpenseTracker.Infrastructure.Mailing.Templates;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace ExpenseTracker.Infrastructure.Mailing.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
            ValidateEmailSettings();
        }

        public async Task<Result> SendEmailConfirmationAsync(string email, string token)
        {
            var confirmationLink = $"https://localhost:4443/api/account/confirm-email?email={email}&confirmEmailToken={token}";
            var subject = "Confirm your email address";
            var body = EmailConfirmationTemplate.GenerateTemplate(email, confirmationLink);

            return await SendEmailAsync(email, subject, body);
        }

        public async Task<Result> SendPasswordResetAsync(string email, string token)
        {
            var resetLink = $"https://localhost:4443/api/account/reset-password?email={email}&resetPasswordToken={token}";
            var subject = "Password reset request";
            var body = PasswordResetTemplate.GenerateTemplate(email, resetLink);

            return await SendEmailAsync(email, subject, body);
        }

        private async Task<Result> SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                var email = CreateMimeMessage(to, subject, body);
                var success = await SendEmailInternalAsync(email);
                if (success)
                {
                    return Result.Success();
                }
                else
                {
                    return Result.Failure(new Dictionary<string, string> { { "EmailError", "Failed to send email." } });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {Email}", to);
                return Result.Failure(new Dictionary<string, string> { { "Exception", ex.Message } });
            }
        }

        private MimeMessage CreateMimeMessage(string to, string subject, string body)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            email.To.Add(new MailboxAddress("", to));
            email.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = body };
            email.Body = bodyBuilder.ToMessageBody();

            return email;
        }

        private async Task<bool> SendEmailInternalAsync(MimeMessage email)
        {
            try
            {
                using var smtp = new MailKit.Net.Smtp.SmtpClient();

                if (_emailSettings.UseStartTls)
                {
                    await smtp.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, SecureSocketOptions.StartTls);
                }
                else if (_emailSettings.UseSSL)
                {
                    await smtp.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, SecureSocketOptions.SslOnConnect);
                }
                else
                {
                    await smtp.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, SecureSocketOptions.Auto);
                }

                await smtp.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);

                _logger.LogInformation("Email sent successfully to {Email}", email.To);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MailKit error while sending email to {Email}", email.To);
                return false;
            }
        }


        private void ValidateEmailSettings()
        {
            if (string.IsNullOrWhiteSpace(_emailSettings.SmtpServer))
                throw new ArgumentException("SMTP server is not configured.", nameof(_emailSettings.SmtpServer));
            if (string.IsNullOrWhiteSpace(_emailSettings.SenderEmail))
                throw new ArgumentException("Sender email is not configured.", nameof(_emailSettings.SenderEmail));
            if (string.IsNullOrWhiteSpace(_emailSettings.Username))
                throw new ArgumentException("SMTP username is not configured.", nameof(_emailSettings.Username));
            if (string.IsNullOrWhiteSpace(_emailSettings.Password))
                throw new ArgumentException("SMTP password is not configured.", nameof(_emailSettings.Password));
        }
    }
}
