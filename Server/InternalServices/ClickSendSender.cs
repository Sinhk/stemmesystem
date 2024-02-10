using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace Stemmesystem.Server.InternalServices
{
    [SuppressMessage("Performance", "CA1848:Use the LoggerMessage delegates")]
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
        
        public async Task<bool> SendSms(string receiver, string message)
        {
            PrepeareRequest();
            var request = new ClickSendSmsRequest();
            request.Messages.Add(new ClickSendMessage("RomNorKrets", receiver, message));
            
            var response = await _client.PostAsJsonAsync("https://rest.clicksend.com/v3/sms/send", request, JsonSerializerOptions);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Sending message to {To} with ClickSend failed with code {StatusCode}", receiver, response.StatusCode);
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

    internal sealed record ClickSendSmsRequest
    {
        public IList<ClickSendMessage> Messages { get; init; } = new List<ClickSendMessage>();
    }

    internal sealed record ClickSendMessage(string From, string To, string Body)
    {
        public string? Source { get; set; }
        public string Country { get; set; } = "NO";

    } 

    public record ClickSendSmsResponse
    {
        [JsonPropertyName("http_code")]
        public string? HttpCode { get; init; }
        [JsonPropertyName("response_code")]
        public string? ResponseCode { get; init; }
        [JsonPropertyName("response_msg")]
        public string? ResponseMessage { get; init; }
    }

    public class ClickSendOptions
    {
        [Required]
        public string? User { get; set; }
        [Required]
        public string? Password { get; set; }
    }
}