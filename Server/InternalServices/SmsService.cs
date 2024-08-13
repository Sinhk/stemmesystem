using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace Stemmesystem.Server.InternalServices;

    public interface ISmsSender
    {
        Task<bool> SendSms(string to, string message);
    }

    internal class SveveSmsSender : ISmsSender
    {
        private readonly HttpClient _httpClient;
        private readonly IOptionsMonitor<SveveOptions> _optionsMonitor;
        private readonly string _baseUrl = "https://sveve.no/SMS/SendMessage";
        private readonly ILogger<SveveSmsSender> _logger;

        public SveveSmsSender(HttpClient httpClient, IOptionsMonitor<SveveOptions> options, ILogger<SveveSmsSender> logger)
        {
            _httpClient = httpClient;
            _optionsMonitor = options;
            _logger = logger;
        }

        private string PrepeareRequest()
        {
            var options = _optionsMonitor.CurrentValue;
            var param = new Dictionary<string, string?>
            {
                {"user", options.User }, 
                {"passwd",options.Password }, 
                {"f","json" },
                {"from", "Speidern" }
            };

            return QueryHelpers.AddQueryString(_baseUrl, param);
        }

        public async Task<bool> SendSms(string to, string message)
        {
            var url = PrepeareRequest();

            url = QueryHelpers.AddQueryString(url, "to", to);
            url = QueryHelpers.AddQueryString(url, "msg", message);

            var httpResponse = await _httpClient.GetAsync(url);
            if (!httpResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Got response code {StatusCode} from sveve", httpResponse.StatusCode);
                try
                {
                    _logger.LogError("Error while sending SMS to Sveve: {SveveError}", await httpResponse.Content.ReadAsStringAsync());
                }
                catch (Exception)
                {
                    // ignored
                }

                return false;
            }

            var responseContainer = await httpResponse.Content.ReadFromJsonAsync<SveveResponseContainer>();
            var response = responseContainer?.Response;
            if (response == null)
                return false;

            if(!string.IsNullOrEmpty(response.FatalError))
                _logger.LogError(response.FatalError);
            if(response.Errors != null)
                foreach (var error in response.Errors)
                {
                    _logger.LogError($"{error.Number}: {error.Message}");    
                }
            
            //TODO: Error handling and more detailed response 
            return string.IsNullOrEmpty(response.FatalError) && (!response.Errors?.Any() ?? true);
        }
    }

    internal record SveveResponseContainer
    {
        [JsonPropertyName("response")]
        public SveveResponse Response { get; init; } = null!;
    }
    internal record SveveResponse
    {
        [JsonPropertyName("msgOkCount")]
        public int MsgOkCount { get; init; }
        [JsonPropertyName("stdSMSCount")]
        public int StdSMSCount { get; init; }
        [JsonPropertyName("fatalError")]
        public string? FatalError { get; init; }
        [JsonPropertyName("errors")]
        public List<SveveError>? Errors { get; init; }
        [JsonPropertyName("ids")]
        public List<int> Ids { get; init; } = null!;

    }

    internal class SveveError
    {
        [JsonPropertyName("number")]
        public string Number { get; init; } = null!;
        [JsonPropertyName("message")]
        public string Message { get; init; } = null!;
    }
    internal class SveveOptions
    {
        [Required]
        public string? User { get; set; }
        [Required]
        public string? Password { get; set; }
    }
