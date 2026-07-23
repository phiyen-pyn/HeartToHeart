public interface IEmailService
{
    // FIXED: Updated method signature to include userId parameter
    Task<bool> SendEmailConfirmationAsync(string email, string firstName, string confirmationToken, Guid userId);
    Task<bool> SendPasswordResetEmailAsync(string email, string firstName, string resetToken);
    Task<bool> SendWelcomeEmailAsync(string email, string firstName);
    Task<bool> SendEmailAsync(string to, string subject, string body);
}