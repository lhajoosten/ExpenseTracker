using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseTracker.Infrastructure.Mailing.Templates
{
    public class PasswordResetTemplate
    {
        // New static method for generating password reset HTML.
        public static string GenerateTemplate(string recipientName, string resetLink)
        {
            return $@"
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
                                <p>Dear {recipientName},</p>
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
        }
    }
}
