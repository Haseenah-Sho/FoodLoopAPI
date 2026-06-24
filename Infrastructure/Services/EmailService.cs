using Application.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace Infrastructure.Services
{
    public class EmailService(IConfiguration configuration) : IEmailService
    {
        public async Task SendVerificationEmailAsync(string toEmail, string verificationToken)
        {
            var subject = "Verify your FoodLoop account";
            var body = $@"
                <h2>Welcome to FoodLoop</h2>
                <p>Thank you for registering. Please verify your email address using the code below:</p>
                <h1 style='letter-spacing: 8px;'>{verificationToken}</h1>
                <p>This code expires in 24 hours.</p>
                <p>If you did not create an account, please ignore this email.</p>";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string resetToken)
        {
            var subject = "FoodLoop password reset code";
            var body = $@"
                <h2>Password Reset Request</h2>
                <p>Use the code below to reset your password:</p>
                <h1 style='letter-spacing: 8px;'>{resetToken}</h1>
                <p>This code expires in 45 minutes.</p>
                <p>If you did not request a password reset, please ignore this email.</p>";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendOrderStatusEmailAsync(string toEmail, string orderNo, string statusTitle, string statusMessage)
        {
            var subject = $"FoodLoop: {statusTitle} — {orderNo}";
            var body = $@"
                <h2>{statusTitle}</h2>
                <p>{statusMessage}</p>
                <p><strong>Order Number:</strong> {orderNo}</p>";

            await SendEmailAsync(toEmail, subject, body);
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var fromEmail = configuration["EmailSettings:From"]!;
            var displayName = configuration["EmailSettings:DisplayName"]!;
            var host = configuration["EmailSettings:Host"]!;
            var port = int.Parse(configuration["EmailSettings:Port"]!);
            var username = configuration["EmailSettings:From"]!;
            var password = configuration["EmailSettings:Password"]!;

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(displayName, fromEmail));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;
            email.Body = new BodyBuilder { HtmlBody = body }.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(host, port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(username, password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}