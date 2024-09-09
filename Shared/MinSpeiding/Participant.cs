using System.Text.Json.Serialization;

namespace Stemmesystem.Shared.MinSpeiding;

public record Participant(
    [property: JsonPropertyName("checked_in")] bool? CheckedIn,
    [property: JsonPropertyName("attended")] bool? Attended,
    [property: JsonPropertyName("cancelled")] bool? Cancelled,
    [property: JsonPropertyName("member_status")] string? MemberStatus,
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
    [property: JsonPropertyName("fee_name")] string? FeeName,
    [property: JsonPropertyName("questions")] IReadOnlyList<object> Questions,
    [property: JsonPropertyName("contact_info")] IReadOnlyDictionary<string, string> ContactInfo
)
{
    public string FullName => string.IsNullOrEmpty(FirstName) ? string.IsNullOrEmpty(LastName) ? "" : LastName : $"{FirstName} {LastName}";
}

public record Labels(
    [property: JsonPropertyName("member_status")] IReadOnlyDictionary<int, string> MemberStatus,
    [property: JsonPropertyName("sex")] IReadOnlyDictionary<int, string> Sex,
    [property: JsonPropertyName("project_fee")] IReadOnlyDictionary<int, string> ProjectFee,
    [property: JsonPropertyName("contact_type")] IReadOnlyDictionary<string, string> ContactType
);

