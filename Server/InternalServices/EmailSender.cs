using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Stemmesystem.Server.InternalServices
{
    public interface IEpostSender
    {
        Task<bool> SendEmailAsync(EpostModel epostModel);
        Task<bool> SendEmailTemplateAsync(string tilEpost, string? tilNavn, string templateId, object data);
    }

    public record EpostModel(string TilEpost, string Subject)
    {
        public string? TilNavn { get; init; }
        public string? PlainTextMessage { get; init; }
        public string? HtmlMessage { get; init; }
    }

    public class EmailSender : IEmailSender, IEpostSender
    {
        private readonly SendMailOptions _options;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IOptions<SendMailOptions> optionsAccessor, ILogger<EmailSender> logger)
        {
            _logger = logger;
            _options = optionsAccessor.Value;
        }
        public async Task SendEmailAsync(string email, string subject, string message)
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

            var result = await client.SendEmailAsync(msg);
            if (!result.IsSuccessStatusCode)
            {
                _logger.LogError("Failed sending email: {message}", await result.Body.ReadAsStringAsync());
            }
        }

        public async Task<bool> SendEmailAsync(EpostModel epostModel)
        {
            var client = new SendGridClient(_options.ApiKey);
            var from = new EmailAddress(_options.FromEmail, _options.FromName);
            var to = new EmailAddress(epostModel.TilEpost,epostModel.TilNavn);
            var msg = MailHelper.CreateSingleEmail(from, to, epostModel.Subject, epostModel.PlainTextMessage, epostModel.HtmlMessage);
            msg.SetClickTracking(false, false);
            var result = await client.SendEmailAsync(msg);
            if (!result.IsSuccessStatusCode)
            {
                _logger.LogError("Failed sending email: {message}", await result.Body.ReadAsStringAsync());
            }
            return result.IsSuccessStatusCode;
        }

        public async Task<bool> SendEmailTemplateAsync(string tilEpost, string? tilNavn, string templateId, object data)
        {
            var client = new SendGridClient(_options.ApiKey);
            var from = new EmailAddress(_options.FromEmail, _options.FromName);
            var to = new EmailAddress(tilEpost,tilNavn);
            var msg = MailHelper.CreateSingleTemplateEmail(from, to, templateId, data);
            msg.SetClickTracking(false, false);
            var result = await client.SendEmailAsync(msg);
            if (!result.IsSuccessStatusCode)
            {
                _logger.LogError("Failed sending email: {message}", await result.Body.ReadAsStringAsync());
            }
            return result.IsSuccessStatusCode;
        }
    }

    public class SendMailOptions
    {
        [Required]
        public string? ApiKey { get; set; }

        [Required] public string? FromEmail { get; set; } = "noreply@romnorkrets.no";

        public string? FromName { get; set; } = "Romsdal og Nordmøre krets";
    }
}