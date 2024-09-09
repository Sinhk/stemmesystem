using System.Text.Json.Nodes;
using FluentAssertions;
using Stemmesystem.Shared.MinSpeiding;
using Xunit;

namespace Stemmesystem.Server.Tests.MinSpeiding;

public class ParticipantHelperTest
{
    [Fact]
    public void Filter_ShouldFilterByArbitraryProperty()
    {
        // Arrange
        var data = JsonNode.Parse(Data)!.AsArray();
        var filter = "first_name=Ola";

        // Act
        var participantFilter = new ParticipantFilter(filter);
        var result = participantFilter.Filter(data);

        // Assert
        result.Should().NotBeNull();
        result.Should().ContainSingle()
            .Which["first_name"]!.GetValue<string>().Should().Be("Ola");
    }

    private static readonly string Data =
        @"[{""checked_in"":true,""attended"":true,""cancelled"":false,""member_status"":""Aktiv"",""member_no"":111111,""group_registration"":true,""first_name"":""Ola"",""last_name"":""Normann"",""ssno"":"""",""registration_date"":""2023-11-15 23:09:42"",""cancelled_date"":null,""sex"":""Mann"",""date_of_birth"":""2001-01-01"",""primary_email"":""xxx@xx.xx"",""group_id"":3002,""patrol_id"":null,""group_name"":""1. Batnfjord speidergruppe"",""org_id"":1,""org_name"":""Norges speiderforbund"",""district_id"":30,""district_name"":""Romsdal og Nordm\u00F8re krets"",""patrol_name"":null,""fee_id"":7036,""questions"":[],""contact_info"":{""Mobiltelefon"":""11111111""},""fee_name"":""Per person for middag""},{""checked_in"":false,""attended"":false,""cancelled"":false,""member_status"":""Aktiv"",""member_no"":111111,""group_registration"":true,""first_name"":""Ole"",""last_name"":""Normann"",""ssno"":"""",""registration_date"":""2023-11-11 17:04:40"",""cancelled_date"":null,""sex"":""Mann"",""date_of_birth"":""2001-01-01"",""primary_email"":""xx@xx.xx"",""group_id"":3003,""patrol_id"":null,""group_name"":""Hustadvika Eide speidergruppe"",""org_id"":1,""org_name"":""Norges speiderforbund"",""district_id"":30,""district_name"":""Romsdal og Nordm\u00F8re krets"",""patrol_name"":null,""fee_id"":7036,""questions"":[],""contact_info"":{""Mobiltelefon"":""11111111"",""Foresatt 1 e-post"":""xx@xx.xx""},""fee_name"":""Per person for middag""},{""checked_in"":true,""attended"":true,""cancelled"":false,""member_status"":""Aktiv"",""member_no"":111111,""group_registration"":true,""first_name"":""Kari"",""last_name"":""Normann"",""ssno"":"""",""registration_date"":""2023-11-13 22:14:08"",""cancelled_date"":null,""sex"":""Mann"",""date_of_birth"":""2001-01-01"",""primary_email"":""xx@xx.xx"",""group_id"":3023,""patrol_id"":null,""group_name"":""Moldespeiderne"",""org_id"":1,""org_name"":""Norges speiderforbund"",""district_id"":30,""district_name"":""Romsdal og Nordm\u00F8re krets"",""patrol_name"":null,""fee_id"":7036,""questions"":[],""contact_info"":{""Mobiltelefon"":""111111111111"",""Foresatt 1 e-post"":""xx.xx@xx.xx"",""Foresatt 2 e-post"":""yy@yy.yy""},""fee_name"":""Per person for middag""}]";
}