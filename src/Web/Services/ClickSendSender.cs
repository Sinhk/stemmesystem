using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Stemmesystem.Web.Services
{
    public class ClickSendSender : ISmsSender
    {
        private readonly HttpClient _client;
        private readonly IOptionsMonitor<ClickSendOptions> _optionsMonitor;
        private readonly ILogger<ClickSendSender> _logger;
        private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web);

        public ClickSendSender(HttpClient client, IOptionsMonitor<ClickSendOptions> optionsMonitor, ILogger<ClickSendSender> logger)
        {
            _client = client;
            _optionsMonitor = optionsMonitor;
            _logger = logger;
        }

        private void PrepeareRequest()
        {
            var options = _optionsMonitor.CurrentValue;
            var authValue = $"Basic { Convert.ToBase64String(Encoding.UTF8.GetBytes($"{options.User}:{options.Password}"))}";
            _client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(authValue);
        }
        
        public async Task<bool> SendSms(string to, string message)
        {
            PrepeareRequest();
            var request = new ClickSendSmsRequest();
            request.Messages.Add(new ClickSendMessage("RomNorKrets", to, message));
            
            var response = await _client.PostAsJsonAsync("https://rest.clicksend.com/v3/sms/send", request, JsonSerializerOptions);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Sending message to {To} with ClickSend failed with code {StatusCode}", to, response.StatusCode);
                try
                {
                    //var errorResponse = await response.Content.ReadFromJsonAsync<ClickSendSmsResponse>(JsonSerializerOptions);
                    _logger.LogError("ClickSend error response {Response}", await response.Content.ReadAsStringAsync());
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed reading error response from ClickSend");
                }
                
                return false;
            }

            var responseBody = await response.Content.ReadFromJsonAsync<ClickSendSmsResponse>(JsonSerializerOptions);
            _logger.LogInformation("Response from ClickSend: {ResponseBody}",responseBody);
            return true;
        }
    }

    internal record ClickSendSmsRequest
    {
        public IList<ClickSendMessage> Messages { get; init; } = new List<ClickSendMessage>();
    }

    internal record ClickSendMessage(string From, string To, string Body)
    {
        public string? Source { get; set; }
        public string Country { get; set; } = "NO";

    } 

    public record ClickSendSmsResponse
    {
        [JsonPropertyName("http_code")]
        public string HttpCode { get; set; }
        [JsonPropertyName("response_code")]
        public string ResponseCode { get; set; }
        [JsonPropertyName("response_msg")]
        public string ResponseMessage { get; set; }
    }

    public class ClickSendOptions
    {
        [Required]
        public string? User { get; set; }
        [Required]
        public string? Password { get; set; }
    }
}