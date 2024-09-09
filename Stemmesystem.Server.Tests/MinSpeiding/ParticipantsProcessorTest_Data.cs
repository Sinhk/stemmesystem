namespace Stemmesystem.Server.Tests.MinSpeiding;

public partial class ParticipantsProcessorTest
{
    private static readonly string ParticipantsData = 
        @"{
  ""participants"": {
    ""280349"": {
      ""checked_in"": true,
      ""attended"": true,
      ""cancelled"": false,
      ""member_status"": 2,
      ""member_no"": 111111,
      ""group_registration"": true,
      ""first_name"": ""Ola"",
      ""last_name"": ""Normann"",
      ""ssno"": """",
      ""registration_date"": ""2023-11-15 23:09:42"",
      ""cancelled_date"": null,
      ""sex"": ""1"",
      ""date_of_birth"": ""2001-01-01"",
      ""primary_email"": ""xxx@xx.xx"",
      ""group_id"": 3002,
      ""patrol_id"": null,
      ""group_name"": ""1. Batnfjord speidergruppe"",
      ""org_id"": 1,
      ""org_name"": ""Norges speiderforbund"",
      ""district_id"": 30,
      ""district_name"": ""Romsdal og Nordmøre krets"",
      ""patrol_name"": null,
      ""fee_id"": 7036,
      ""questions"": [],
      ""contact_info"": {
        ""1"": ""11111111""
      }
    },
    ""280607"": {
      ""checked_in"": false,
      ""attended"": false,
      ""cancelled"": false,
      ""member_status"": 2,
      ""member_no"": 111111,
      ""group_registration"": true,
      ""first_name"": ""Ole"",
      ""last_name"": ""Normann"",
      ""ssno"": """",
      ""registration_date"": ""2023-11-11 17:04:40"",
      ""cancelled_date"": null,
      ""sex"": ""1"",
      ""date_of_birth"": ""2001-01-01"",
      ""primary_email"": ""xx@xx.xx"",
      ""group_id"": 3003,
      ""patrol_id"": null,
      ""group_name"": ""Hustadvika Eide speidergruppe"",
      ""org_id"": 1,
      ""org_name"": ""Norges speiderforbund"",
      ""district_id"": 30,
      ""district_name"": ""Romsdal og Nordmøre krets"",
      ""patrol_name"": null,
      ""fee_id"": 7036,
      ""questions"": [],
      ""contact_info"": {
        ""1"": ""11111111"",
        ""12"": ""xx@xx.xx""
      }
    },
    ""281363"": {
      ""checked_in"": true,
      ""attended"": true,
      ""cancelled"": false,
      ""member_status"": 2,
      ""member_no"": 111111,
      ""group_registration"": true,
      ""first_name"": ""Kari"",
      ""last_name"": ""Normann"",
      ""ssno"": """",
      ""registration_date"": ""2023-11-13 22:14:08"",
      ""cancelled_date"": null,
      ""sex"": ""1"",
      ""date_of_birth"": ""2001-01-01"",
      ""primary_email"": ""xx@xx.xx"",
      ""group_id"": 3023,
      ""patrol_id"": null,
      ""group_name"": ""Moldespeiderne"",
      ""org_id"": 1,
      ""org_name"": ""Norges speiderforbund"",
      ""district_id"": 30,
      ""district_name"": ""Romsdal og Nordmøre krets"",
      ""patrol_name"": null,
      ""fee_id"": 7036,
      ""questions"": [],
      ""contact_info"": {
        ""1"": ""111111111111"",
        ""12"": ""xx.xx@xx.xx"",
        ""54"": ""yy@yy.yy""
      }
    }
  },
  ""labels"": {
    ""member_status"": {
      ""1"": ""Utmeldt"",
      ""2"": ""Aktiv"",
      ""4"": ""Ny, ikke klar for fakturering"",
      ""8"": ""Død"",
      ""16"": ""Automatisk utmeldt"",
      ""32"": ""Aktiv ikke-medlem"",
      ""64"": ""Anonymisert""
    },
    ""sex"": {
      ""0"": ""Ukjent"",
      ""1"": ""Mann"",
      ""2"": ""Kvinne"",
      ""3"": ""Annet""
    },
    ""project_fee"": {
      ""7035"": ""Standardpris"",
      ""7036"": ""Per person for middag""
    },
    ""contact_type"": {
      ""1"": ""Mobiltelefon"",
      ""9"": ""E-post2"",
      ""12"": ""Foresatt 1 e-post"",
      ""54"": ""Foresatt 2 e-post""
    }
  }
}";
}