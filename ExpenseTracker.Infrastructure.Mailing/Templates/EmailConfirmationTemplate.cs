namespace ExpenseTracker.Infrastructure.Mailing.Templates
{
    public class EmailConfirmationTemplate
    {
        public static string GenerateTemplate(string recipientName, string confirmationLink)
        {
            return $@"
                        <!DOCTYPE html>
                        <html>
                        <head>
                            <meta charset='UTF-8'>
                            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                            <style>
                                body {{
                                    font-family: 'Segoe UI', Arial, sans-serif;
                                    line-height: 1.6;
                                    margin: 0;
                                    padding: 0;
                                    background-color: #f5f5f5;
                                }}
                                .container {{
                                    max-width: 600px;
                                    margin: 20px auto;
                                    padding: 30px;
                                    background-color: #ffffff;
                                    border-radius: 12px;
                                    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
                                }}
                                .logo {{
                                    text-align: center;
                                    margin-bottom: 30px;
                                }}
                                .header {{
                                    text-align: center;
                                    padding-bottom: 25px;
                                    border-bottom: 2px solid #f0f0f0;
                                }}
                                .header h1 {{
                                    color: #2C3E50;
                                    font-size: 28px;
                                    margin: 0;
                                }}
                                .content {{
                                    margin-top: 25px;
                                    color: #444444;
                                }}
                                .button {{
                                    display: inline-block;
                                    padding: 12px 30px;
                                    margin: 25px 0;
                                    background-color: #3498DB;
                                    color: #ffffff;
                                    text-decoration: none;
                                    border-radius: 6px;
                                    font-weight: bold;
                                    text-align: center;
                                }}
                                .button:hover {{
                                    background-color: #2980B9;
                                }}
                                .footer {{
                                    margin-top: 30px;
                                    padding-top: 20px;
                                    border-top: 2px solid #f0f0f0;
                                    text-align: center;
                                    font-size: 0.9em;
                                    color: #888888;
                                }}
                                .notice {{
                                    font-size: 0.8em;
                                    color: #999999;
                                    margin-top: 20px;
                                }}
                            </style>
                        </head>
                        <body>
                            <div class='container'>
                                <div class='logo'>
                                    <!-- Replace with your logo -->
                                    <h2 style='color: #3498DB;'>ExpenseTracker</h2>
                                </div>
                                <div class='header'>
                                    <h1>Welcome to ExpenseTracker!</h1>
                                </div>
                                <div class='content'>
                                    <p>Hello {recipientName},</p>
                                    <p>Thank you for creating an account with ExpenseTracker. We're excited to have you on board! To start managing your expenses, please verify your email address by clicking the button below:</p>
            
                                    <div style='text-align: center;'>
                                        <a href='{confirmationLink}' class='button'>Verify Email Address</a>
                                    </div>

                                    <p>If the button doesn't work, you can copy and paste this link into your browser:</p>
                                    <p style='font-size: 0.9em; color: #666666;'>{confirmationLink}</p>
            
                                    <p>For security reasons, this link will expire in 24 hours.</p>
                                </div>
                                <div class='footer'>
                                    <p>Need help? Contact our support team at support@expensetracker.com</p>
                                    <div class='notice'>
                                        <p>This is an automated message, please do not reply to this email.</p>
                                        <p>&copy; {DateTime.Now.Year} ExpenseTracker. All rights reserved.</p>
                                    </div>
                                </div>
                            </div>
                        </body>
                        </html>";
        }
    }
}

