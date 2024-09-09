using System.Net;
using System.Text.Json.Nodes;
using LanguageExt.Common;
using Microsoft.EntityFrameworkCore;
using Stemmesystem.Data;

namespace Stemmesystem.Server.Features.MinSpeiding;

public class MinSpeidingService
{
    private readonly HttpClient _httpClient;
    private readonly StemmesystemContext _dbContext;
    private readonly ILogger<MinSpeidingService> _logger;
    private readonly Uri _baseUrl = new("https://min.speiding.no/api/");

    public MinSpeidingService(HttpClient httpClient, StemmesystemContext dbContext, ILogger<MinSpeidingService> logger)
    {
        _httpClient = httpClient;
        _dbContext = dbContext;
        _logger = logger;
    }


    /// <summary>
    /// Get participants from MinSpeiding using internal arrangementId to fetch options
    /// </summary>
    /// <param name="arrangementId"></param>
    /// <returns></returns>
    public async Task<Result<JsonArray?>> GetArrangementParticipants(int arrangementId)
    {
        var options = await _dbContext.Arrangement.AsNoTracking()
            .Where(a => a.Id == arrangementId)
            .Select(x => x.MinSpeidingOptions)
            .SingleOrDefaultAsync();

        if (options == null)
            return new Result<JsonArray?>(new MinSpeidingException("Min speiding ikke konfigurert for arrangementet"));

        if (options.MinSpeidingId is 0)
            return new Result<JsonArray?>(new MinSpeidingException("Min speiding arrangement ID mangler")); 
        if (string.IsNullOrEmpty(options.MembersApiKey))
            return new Result<JsonArray?>(new MinSpeidingException("Min speiding API nøkkel mangler"));
        
        return await GetArrangementParticipants(options.MinSpeidingId, options.MembersApiKey);
    }

    /// <summary>
    /// Get participants from Min speiding, using Min speiding arrangement ID and apiKey
    /// </summary>
    /// <param name="arrangementId"></param>
    /// <param name="apiKey"></param>
    /// <returns></returns>
    public async Task<Result<JsonArray?>> GetArrangementParticipants(int arrangementId, string? apiKey)
    {
        var path = $"project/get/participants?id={arrangementId}&key={apiKey}";
        var httpResponse = await _httpClient.GetAsync(new Uri(_baseUrl,path));
        if (!httpResponse.IsSuccessStatusCode)
            return new Result<JsonArray?>(HandleError(httpResponse));
            
        var response = await httpResponse.Content.ReadFromJsonAsync<JsonNode>();
        return ParticipantsProcessor.ParseParticipants(response);
    }

    private static Exception HandleError(HttpResponseMessage httpResponse) => httpResponse switch
        {
            { StatusCode: HttpStatusCode.NotFound } 
                => new MinSpeidingException("Min speiding arrangement ikke funnet"),
            { StatusCode: HttpStatusCode.Unauthorized } 
                => new MinSpeidingException("Feil eller manglende API nøkkel for Min speiding"),
            { StatusCode: HttpStatusCode.BadRequest } 
                => new MinSpeidingException("Feil i forespørsel til Min speiding"),
            _ => new MinSpeidingException("Feil ved henting av data fra Min speiding")
        };
}

internal class MinSpeidingException : Exception
{
    public MinSpeidingException(string? message) : base(message) { }
}