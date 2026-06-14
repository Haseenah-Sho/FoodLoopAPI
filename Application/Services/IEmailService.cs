namespace Application.Services
{
    public interface IEmailService
    {
        Task SendVerificationEmailAsync(string toEmail, string verificationToken);
        Task SendPasswordResetEmailAsync(string toEmail, string resetToken);
    }
}