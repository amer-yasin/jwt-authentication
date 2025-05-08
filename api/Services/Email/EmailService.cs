using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace api.Services
{
    public class EmailService : IEmailService
    {
        private readonly string _smtpHost = "127.0.0.1"; // Replace with your SMTP host
        private readonly int _smtpPort = 1025; // Replace with your SMTP port

        public async Task SendEmailAsync(string recipientEmail, string subject, string body)
        {
            using (var client = new SmtpClient(_smtpHost, _smtpPort))
            {
                // No authentication or SSL required
                client.UseDefaultCredentials = true;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("no-reply@example.com"), // Replace with a valid sender email
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(recipientEmail);

                await client.SendMailAsync(mailMessage);
            }
        }
    }
}