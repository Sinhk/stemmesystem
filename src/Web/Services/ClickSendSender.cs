using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using IO.ClickSend.ClickSend.Api;
using IO.ClickSend.ClickSend.Model;
using IO.ClickSend.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Stemmesystem.Web.Services
{
    public class ClickSendSender : ISmsSender
    {
        private readonly IOptionsMonitor<ClickSendOptions> _optionsMonitor;
        private readonly ILogger<ClickSendSender> _logger;

        public ClickSendSender(IOptionsMonitor<ClickSendOptions> optionsMonitor, ILogger<ClickSendSender> logger)
        {
            _optionsMonitor = optionsMonitor;
            _logger = logger;
        }

        public async Task<bool> SendSms(string to, string message)
        {
            var options = _optionsMonitor.CurrentValue;
            var configuration = new Configuration {Username = options.User,Password = options.Password};
            var api = new SMSApi(configuration);

            SmsMessageCollection messages = new(new List<SmsMessage> {new("RomNorKrets",message,to,country:"NO")});
            var response = await api.SmsSendPostAsyncWithHttpInfo(messages);
            
            _logger.LogInformation(response.Data);
            return response.StatusCode == 200;
        }
    }

    public class ClickSendOptions
    {
        [Required]
        public string? User { get; set; }
        [Required]
        public string? Password { get; set; }
    }
}