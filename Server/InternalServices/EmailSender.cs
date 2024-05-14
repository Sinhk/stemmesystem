using System.ComponentModel.DataAnnotations;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Stemmesystem.Server.InternalServices
{
    public interface IEpostSender
    {
        Task<bool> SendEmailAsync(EpostModel epostModel);
    }

    public record EpostModel(string TilEpost, string Subject)
    {
        public string? TilNavn { get; init; }
        public string? PlainTextMessage { get; init; }
        public string? HtmlMessage { get; init; }
    }

    public class EmailSender : IEmailSender, IEpostSender
    {
        private readonly EmailSettings _options;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IOptions<EmailSettings> optionsAccessor, ILogger<EmailSender> logger)
        {
            _logger = logger;
            _options = optionsAccessor.Value;
        }
        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var msg = new MimeMessage();
            msg.From.Add(new MailboxAddress(_options.FromName, _options.FromEmail));
            msg.To.Add(InternetAddress.Parse(email));
            msg.Subject = subject;
            msg.Body = new TextPart("plain") { Text = message };

            using var client = await GetAuthenticatedClient();
            await client.SendAsync(msg);
            await client.DisconnectAsync(true);

            /*if (!result.IsSuccessStatusCode)
            {
                _logger.LogError("Failed sending email: {message}", await result.Body.ReadAsStringAsync());
            }*/
        }

        public async Task<bool> SendEmailAsync(EpostModel epostModel)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_options.FromName, _options.FromEmail));
            message.To.Add(new MailboxAddress(epostModel.TilNavn, epostModel.TilEpost));
            message.Subject = epostModel.Subject;
            
            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = epostModel.HtmlMessage,
                TextBody = epostModel.PlainTextMessage
            };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = await GetAuthenticatedClient();
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            return true;
        }

        private async Task<SmtpClient> GetAuthenticatedClient()
        {
            var client = new SmtpClient();
            await client.ConnectAsync(_options.Host, _options.Port, true);
            await client.AuthenticateAsync(_options.Username, _options.Password);
            return client;
        }
    }

    public class EmailSettings
    {
        private string? _fromEmail;

        public string? FromEmail
        {
            get => _fromEmail ?? Username;
            set => _fromEmail = value;
        }

        public string? FromName { get; set; }
        [Required] public string Host { get; set; } = "smtp.gmail.com";
        public int Port { get; set; } = 587;

        [Required] public string Username { get; set; } = null!;

        [Required]public string Password { get; set; } = null!;
    }
}