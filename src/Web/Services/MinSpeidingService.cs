using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Stemmesystem.Web
{
    internal class MinSpeidingService
    {
        private readonly HttpClient _httpClient;
        private readonly IOptionsMonitor<MinSpeidingOptions> _optionsMonitor;
        private readonly IMemoryCache _cache;
        private readonly Uri _baseUrl = new("https://min.speiding.no/api/");

        public MinSpeidingService(HttpClient httpClient, IOptionsMonitor<MinSpeidingOptions> options, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _optionsMonitor = options;
            _cache = cache;
        }

        private string PrepeareRequest(string relativeUrl)
        {
            var options = _optionsMonitor.CurrentValue;
            var param = new Dictionary<string, string?>
            {
                {"id", options.GroupId }, 
                {"key",options.ApiKey }
            };

            return QueryHelpers.AddQueryString(new Uri(_baseUrl,relativeUrl).ToString(), param);
        }

        public async Task<IEnumerable<MinSpeidingMember>> LookupMembers(string query)
        {
            var members = await GetAllMembers();
            return members.Where(m => m.FirstName.Value.Contains(query, StringComparison.InvariantCultureIgnoreCase) || m.LastName.Value.Contains(query, StringComparison.InvariantCultureIgnoreCase));
        }

        internal Task<List<MinSpeidingMember>?> GetAllMembers()
        {
            return _cache.GetOrCreateAsync("min-speiding-members", async e =>
            {
                e.SlidingExpiration = TimeSpan.FromSeconds(2);
                var url = PrepeareRequest("group/memberlist");

                var httpResponse = await _httpClient.GetAsync(url);
                if (!httpResponse.IsSuccessStatusCode)
                    return null;
            
                var response = await httpResponse.Content.ReadFromJsonAsync<MinSpeidingMembers>();
                if (response == null)
                    return null;
                e.SlidingExpiration = TimeSpan.FromMinutes(15);
                return response.Data.Values.ToList();    
            });
        }
    }

    internal class MinSpeidingOptions
    {
        [Required]
        public string? GroupId { get; set; }
        [Required]
        public string? ApiKey { get; set; }
    }

    internal record MinSpeidingMembers
    {
        [JsonPropertyName("data")] 
        public Dictionary<int,MinSpeidingMember> Data { get; set; }
        [JsonPropertyName("labels")]
        public Dictionary<string,string> Labels { get; set; }
    }

    internal record MinSpeidingMember
    {
        [JsonPropertyName("member_no")]
        public MinSpeidingValue MemberNo { get; set; }
        [JsonPropertyName("first_name")]
        public MinSpeidingValue FirstName { get; set; }
        [JsonPropertyName("last_name")]
        public MinSpeidingValue LastName { get; set; }
        [JsonPropertyName("email")]
        public MinSpeidingValue Email { get; set; }
        [JsonPropertyName("group")]
        public MinSpeidingValue Group { get; set; }
        [JsonPropertyName("contact_mobile_phone")]
        public MinSpeidingValue Phone { get; set; }
    }
    internal record MinSpeidingValue
    {
        [JsonPropertyName("value")]
        public string Value { get; set; }
    }
}