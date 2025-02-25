using ExpenseTracker.Common.Abstractions;
using ExpenseTracker.Common.Models;
using ExpenseTracker.Infrastructure.Mailing.Models;
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

        public async Task<Result> SendEmailAsync(string to, string subject, string body)
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

        public async Task<Result> SendEmailConfirmationAsync(string email, string token)
        {
            var confirmationLink = $"https://localhost:5001/api/account/confirm-email?email={email}&token={token}";
            var subject = "Confirm your email address";
            var body = $@"
                    <html>
                    <head>
                        <style>
                            body {{
                                font-family: Arial, sans-serif;
                                line-height: 1.6;
                            }}
                            .container {{
                                width: 80%;
                                margin: auto;
                                padding: 20px;
                                border: 1px solid #ccc;
                                border-radius: 10px;
                                background-color: #f9f9f9;
                            }}
                            .header {{
                                text-align: center;
                                padding-bottom: 20px;
                            }}
                            .content {{
                                margin-top: 20px;
                            }}
                            .footer {{
                                margin-top: 30px;
                                text-align: center;
                                font-size: 0.9em;
                                color: #777;
                            }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <h1>Email Confirmation</h1>
                            </div>
                            <div class='content'>
                                <p>Dear User,</p>
                                <p>Please confirm your email address by clicking the link below:</p>
                                <p><a href='{confirmationLink}'>Confirm Email</a></p>
                            </div>
                            <div class='footer'>
                                <p>Please do not reply to this email. This is an automated message.</p>
                            </div>
                        </div>
                    </body>
                    </html>";

            return await SendEmailAsync(email, subject, body);
        }

        public Task<Result> SendPasswordResetAsync(string email, string token)
        {
            var resetLink = $"https://localhost:5001/api/account/reset-password?email={email}&token={token}";
            var subject = "Password reset request";
            var body = $@"
                    <html>
                    <head>
                        <style>
                            body {{
                                font-family: Arial, sans-serif;
                                line-height: 1.6;
                            }}
                            .container {{
                                width: 80%;
                                margin: auto;
                                padding: 20px;
                                border: 1px solid #ccc;
                                border-radius: 10px;
                                background-color: #f9f9f9;
                            }}
                            .header {{
                                text-align: center;
                                padding-bottom: 20px;
                            }}
                            .content {{
                                margin-top: 20px;
                            }}
                            .footer {{
                                margin-top: 30px;
                                text-align: center;
                                font-size: 0.9em;
                                color: #777;
                            }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <h1>Password Reset</h1>
                            </div>
                            <div class='content'>
                                <p>Dear User,</p>
                                <p>We received a request to reset your password. If you did not make this request, please ignore this email.</p>
                                <p>To reset your password, click the link below:</p>
                                <p><a href='{resetLink}'>Reset Password</a></p>
                            </div>
                            <div class='footer'>
                                <p>Please do not reply to this email. This is an automated message.</p>
                            </div>
                        </div>
                    </body>
                    </html>";

            return SendEmailAsync(email, subject, body);
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
