namespace TravelPlanner.Services
{
    /// <summary>
    /// Email Service Interface
    /// </summary>
    public interface IEmailService
    {
        Task SendEmailAsync(string email, string subject, string htmlMessage);
        Task SendConfirmationEmailAsync(string email, string confirmationLink);
        Task SendPasswordResetEmailAsync(string email, string resetLink);
    }
}
