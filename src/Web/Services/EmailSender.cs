using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Stemmesystem.Web.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly SendMailOptions _options;

        public EmailSender(IOptions<SendMailOptions> optionsAccessor)
        {
            _options = optionsAccessor.Value;
        }
        public Task SendEmailAsync(string email, string subject, string message)
        {
            var client = new SendGridClient(_options.ApiKey);
            var msg = new SendGridMessage
            {
                From = new EmailAddress(_options.FromEmail, _options.FromName),
                Subject = subject,
                PlainTextContent = message,
                HtmlContent = message
            };
            msg.AddTo(new EmailAddress(email));

            //var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            // Disable click tracking.
            // See https://sendgrid.com/docs/User_Guide/Settings/tracking.html
            msg.SetClickTracking(false, false);

            return client.SendEmailAsync(msg);
        }
    }
    
    public class SendMailOptions
    {
        [Required]
        public string? ApiKey { get; set; }
        [Required]
        public string? FromEmail { get; set; }

        public string? FromName { get; set; }
    }

}