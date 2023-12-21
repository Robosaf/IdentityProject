using IdentityProject.Helpers;
using IdentityProject.Interfaces;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace IdentityProject.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly AuthMessageSenderOptions _options;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IOptions<AuthMessageSenderOptions> options, ILogger<EmailSender> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(_options.Email, _options.Password)
            };

            MailMessage mailMessage = new MailMessage(from: _options.Email,
                to: toEmail,
                subject: subject,
                body: message);

            var response = client.SendMailAsync(mailMessage);

            _logger.LogInformation(response.IsCompletedSuccessfully
                                   ? $"Email to {toEmail} queued successfully!"
                                   : $"Failure Email to {toEmail}");
        }

    }
}
