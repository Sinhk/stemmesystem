using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Path;
using Stemmesystem.Shared.MinSpeiding;

namespace Stemmesystem.Server.Features.MinSpeiding;

public class ParticipantsProcessor
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static JsonArray? ParseParticipants(JsonNode? rawData)
    {
        if (rawData is null)
            return null;

        var labels = rawData.AsObject()["labels"].Deserialize<Labels>();
        if (labels is null)
            return null;

        var path = JsonPath.Parse("$.participants[*]");
        var pathResult = path.Evaluate(rawData);

        var result = new JsonArray();

        foreach (var match in pathResult.Matches.Select(m => m.Value!.AsObject()))
        {
            var participant = match.DeepClone().AsObject();
            MapFee(participant);
            MapSex(participant);
            MapMemberStatus(participant);
            MapContactInfo(participant);
            MapQuestions(participant);

            result.Add(participant);
        }

        return result;


        void MapFee(JsonObject participant)
        {
            if (!participant.TryGetPropertyValue("fee_id", out var feeIdRaw) || feeIdRaw is null) return;
            if (labels.ProjectFee.TryGetValue(feeIdRaw.GetValue<int>(), out var feeName))
            {
                participant.Add("fee_name", feeName);
            }
        }

        void MapSex(JsonObject participant)
        {
            if (!participant.TryGetPropertyValue("sex", out var sexRaw) || sexRaw is null) return;
            if (labels.Sex.TryGetValue(sexRaw.Deserialize<int>(JsonOptions), out var sexName))
            {
                participant["sex"] = sexName;
            }
        }

        void MapMemberStatus(JsonObject participant)
        {
            if (!participant.TryGetPropertyValue("member_status", out var memberStatusRaw) ||
                memberStatusRaw is null) return;
            if (labels.MemberStatus.TryGetValue(memberStatusRaw.GetValue<int>(), out var memberStatusName))
            {
                participant["member_status"] = memberStatusName;
            }
        }

        void MapContactInfo(JsonObject participant)
        {
            if (!participant.TryGetPropertyValue("contact_info", out var contactInfoRaw) || contactInfoRaw is not JsonObject contactInfo) return;
            foreach (var (key, value) in contactInfo.ToArray())
            {
                if (!labels.ContactType.TryGetValue(key, out var name)) continue;
                contactInfo.Remove(key);
                contactInfo.Add(name, value);
            }
        }
        void MapQuestions(JsonObject participant)
        {
            const string propertyName = "questions";
            if (!participant.TryGetPropertyValue(propertyName, out var questionsRaw)) return;
            if (questionsRaw is JsonArray)
            {
                // must be object for deserialization to work
                participant.Remove(propertyName);
                participant.Add(propertyName, new JsonObject());
                return;
            }
            //TODO: Get questions from MinSpeiding
        }
    }
}