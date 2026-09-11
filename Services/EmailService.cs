using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;

namespace TravelPlanner.Services
{
    /// <summary>
    /// Email Service Implementation using MailKit
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var port = int.Parse(_configuration["EmailSettings:Port"]);
                var senderEmail = _configuration["EmailSettings:SenderEmail"];
                var senderPassword = _configuration["EmailSettings:SenderPassword"];

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Travel Planner", senderEmail));
                message.To.Add(new MailboxAddress("", email));
                message.Subject = subject;
                message.Body = new TextPart(TextFormat.Html) { Text = htmlMessage };

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(smtpServer, port, false);
                    await client.AuthenticateAsync(senderEmail, senderPassword);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }

                _logger.LogInformation($"Email sent successfully to {email}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending email: {ex.Message}");
                throw;
            }
        }

        public async Task SendConfirmationEmailAsync(string email, string confirmationLink)
        {
            var subject = "Confirm Your Travel Planner Account";
            var htmlMessage = $@"
                <html>
                <body style='font-family: Arial, sans-serif; background-color: #f5f5f5; padding: 20px;'>
                    <div style='max-width: 600px; margin: 0 auto; background-color: white; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 8px rgba(0,0,0,0.1);'>
                        <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; text-align: center; color: white;'>
                            <h1 style='margin: 0; font-size: 28px;'>Welcome to Travel Planner!</h1>
                        </div>
                        <div style='padding: 30px;'>
                            <h2 style='color: #333; margin-bottom: 20px;'>Confirm Your Email Address</h2>
                            <p style='color: #666; line-height: 1.6; margin-bottom: 20px;'>
                                Thank you for signing up! Please confirm your email address by clicking the button below.
                            </p>
                            <a href='{confirmationLink}' style='display: inline-block; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 12px 30px; border-radius: 5px; text-decoration: none; font-weight: bold; margin: 20px 0;'>
                                Confirm Email
                            </a>
                            <p style='color: #999; font-size: 12px; margin-top: 30px;'>
                                This link will expire in 24 hours.
                            </p>
                        </div>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(email, subject, htmlMessage);
        }

        public async Task SendPasswordResetEmailAsync(string email, string resetLink)
        {
            var subject = "Reset Your Travel Planner Password";
            var htmlMessage = $@"
                <html>
                <body style='font-family: Arial, sans-serif; background-color: #f5f5f5; padding: 20px;'>
                    <div style='max-width: 600px; margin: 0 auto; background-color: white; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 8px rgba(0,0,0,0.1);'>
                        <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; text-align: center; color: white;'>
                            <h1 style='margin: 0; font-size: 28px;'>Password Reset</h1>
                        </div>
                        <div style='padding: 30px;'>
                            <h2 style='color: #333; margin-bottom: 20px;'>Reset Your Password</h2>
                            <p style='color: #666; line-height: 1.6; margin-bottom: 20px;'>
                                We received a request to reset your password. Click the button below to create a new password.
                            </p>
                            <a href='{resetLink}' style='display: inline-block; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 12px 30px; border-radius: 5px; text-decoration: none; font-weight: bold; margin: 20px 0;'>
                                Reset Password
                            </a>
                            <p style='color: #999; font-size: 12px; margin-top: 30px;'>
                                This link will expire in 1 hour. If you didn't request this, ignore this email.
                            </p>
                        </div>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(email, subject, htmlMessage);
        }
    }
}
