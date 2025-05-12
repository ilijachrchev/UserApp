using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace UserApp.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(
            IConfiguration configuration,
            ILogger<EmailSender> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                var smtpHost = _configuration["Smtp:Host"] ?? throw new InvalidOperationException("Smtp:Host is not configured.");
                var smtpPort = int.TryParse(_configuration["Smtp:Port"], out int port) ? port : throw new InvalidOperationException("Smtp:Port is not configured or invalid.");
                var smtpUser = _configuration["Smtp:Username"] ?? throw new InvalidOperationException("Smtp:Username is not configured.");
                var smtpPass = _configuration["Smtp:Password"] ?? throw new InvalidOperationException("Smtp:Password is not configured.");
                var fromEmail = _configuration["Smtp:From"] ?? throw new InvalidOperationException("Smtp:From is not configured.");

                var message = new MailMessage();
                message.From = new MailAddress(fromEmail, "From Church's Backend MASTERY");
                message.To.Add(new MailAddress(email));
                message.Subject = subject;
                message.Body = htmlMessage;
                message.IsBodyHtml = true;

                try
                {
                    using (var client = new SmtpClient(smtpHost, smtpPort))
                    {
                        client.UseDefaultCredentials = false;
                        client.Credentials = new NetworkCredential(smtpUser, smtpPass);
                        client.EnableSsl = true;
                        client.DeliveryMethod = SmtpDeliveryMethod.Network;

                        if (message == null)
                        {
                            throw new ArgumentNullException(nameof(message));
                        }

                        await client.SendMailAsync(message);
                    }
                    _logger.LogInformation($"Email sent successfully to {email} with subject '{subject}'.");
                    return true;
                }
                catch (SmtpException smtpEx)
                {
                    _logger.LogError(smtpEx, "SMTP ERROR: " + smtpEx.Message);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EMAIL ERROR: " + ex.Message);
                throw;
            }
        }
    }
}
