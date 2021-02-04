using System.Text.Json.Serialization;
using Stemmesystem.Server.Data.Entities;

namespace Stemmesystem.Server.Services;

internal class MinSpeidingService
{
    private readonly HttpClient _httpClient;
    private readonly Uri _baseUrl = new("https://min.speiding.no/api/");

    public MinSpeidingService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyCollection<Participant>?> GetArrangementParticipants(MinSpeidingApiOptions apiOptions)
    {
        var path = $"project/get/participants?id={apiOptions.ArrangementId}key={apiOptions.ApiKey}";
        var httpResponse = await _httpClient.GetAsync(new Uri(_baseUrl,path));
        if (!httpResponse.IsSuccessStatusCode)
            return null;
            
        var response = await httpResponse.Content.ReadFromJsonAsync<ArrangementParticipants>();
        if (response == null)
            return null;
        
        return [..response.Participants.Values];
    }
}

internal record ArrangementParticipants
{
    public IReadOnlyDictionary<int, Participant> Participants { get; init; }
}

public record Participant(
    [property: JsonPropertyName("checked_in")] bool? CheckedIn,
    [property: JsonPropertyName("attended")] bool? Attended,
    [property: JsonPropertyName("cancelled")] bool? Cancelled,
    [property: JsonPropertyName("member_status")] int? MemberStatus,
    [property: JsonPropertyName("member_no")] int? MemberNo,
    [property: JsonPropertyName("group_registration")] bool? GroupRegistration,
    [property: JsonPropertyName("first_name")] string FirstName,
    [property: JsonPropertyName("last_name")] string LastName,
    [property: JsonPropertyName("ssno")] string Ssno,
    [property: JsonPropertyName("registration_date")] string RegistrationDate,
    [property: JsonPropertyName("cancelled_date")] object CancelledDate,
    [property: JsonPropertyName("sex")] string Sex,
    [property: JsonPropertyName("date_of_birth")] string DateOfBirth,
    [property: JsonPropertyName("primary_email")] string PrimaryEmail,
    [property: JsonPropertyName("group_id")] int? GroupId,
    [property: JsonPropertyName("patrol_id")] object PatrolId,
    [property: JsonPropertyName("group_name")] string GroupName,
    [property: JsonPropertyName("org_id")] int? OrgId,
    [property: JsonPropertyName("org_name")] string OrgName,
    [property: JsonPropertyName("district_id")] int? DistrictId,
    [property: JsonPropertyName("district_name")] string DistrictName,
    [property: JsonPropertyName("patrol_name")] object PatrolName,
    [property: JsonPropertyName("fee_id")] int? FeeId,
    [property: JsonPropertyName("questions")] IReadOnlyList<object> Questions,
    [property: JsonPropertyName("contact_info")] IReadOnlyDictionary<string, string> ContactInfo
);



internal record MinSpeidingMembers
{
    [JsonPropertyName("data")] 
    public Dictionary<int,MinSpeidingMember> Data { get; init; }
    [JsonPropertyName("labels")]
    public Dictionary<string,string> Labels { get; init; }
}

internal record MinSpeidingMember
{
    [JsonPropertyName("member_no")]
    public MinSpeidingValue MemberNo { get; init; }
    [JsonPropertyName("first_name")]
    public MinSpeidingValue FirstName { get; init; }
    [JsonPropertyName("last_name")]
    public MinSpeidingValue LastName { get; init; }
    [JsonPropertyName("email")]
    public MinSpeidingValue Email { get; init; }
    [JsonPropertyName("group")]
    public MinSpeidingValue Group { get; init; }
    [JsonPropertyName("contact_mobile_phone")]
    public MinSpeidingValue Phone { get; init; }
}
internal record MinSpeidingValue
{
    [JsonPropertyName("value")]
    public string Value { get; init; }
}