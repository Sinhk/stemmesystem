using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using IO.ClickSend.ClickSend.Api;
using IO.ClickSend.ClickSend.Model;
using IO.ClickSend.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Stemmesystem.Web
{
    internal class ClickSendSender : ISmsSender
    {
        private readonly ClickSendOptions _options;
        private readonly ILogger<ClickSendSender> _logger;

        public ClickSendSender(IOptions<ClickSendOptions> options, ILogger<ClickSendSender> logger)
        {
            _logger = logger;
            _options = options.Value;
        }
        public async Task<bool> SendSms(string to, string message)
        {
            var configuration = new Configuration {Username = _options.User,Password = _options.Password};
            var api = new SMSApi(configuration);

            SmsMessageCollection messages = new(new List<SmsMessage> {new("RomNorKrets",message,to,country:"NO")});
            var response = await api.SmsSendPostAsyncWithHttpInfo(messages);
            
            _logger.LogInformation(response.Data);
            return response.StatusCode == 200;
        }
    }
    
    internal class ClickSendOptions
    {
        [Required]
        public string? User { get; set; }
        [Required]
        public string? Password { get; set; }
    }
}