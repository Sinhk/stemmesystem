using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Path;
using Json.Pointer;

namespace Stemmesystem.Shared.MinSpeiding;

public class ParticipantFilter
{
    private static readonly JsonNodeOptions NodeOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public string RawFilter { get; }

    private readonly List<JsonFilter> _filters = new();


    public ParticipantFilter(string expression)
    {
        RawFilter = expression;

        var filters = expression.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        foreach (var filter in filters)
        {
            var parts = filter.Split('=', StringSplitOptions.TrimEntries);
            if (parts.Length != 2)
            {
                throw new ArgumentException("Invalid filter expression", nameof(expression));
            }
            
            var jsonFilter = JsonFilter.Create(parts[0], parts[1]);
            _filters.Add(jsonFilter);
        }
    }

    private record JsonFilter(JsonPointer Key, string[] Values)
    {

        public static JsonFilter Create(string key, string value)
        {
            
            
            var pointer = JsonPointer.Parse("/" + key.Replace('.','/'));
            return new JsonFilter(pointer, value.Split(',',StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));
        }

        public bool IsMatch(JsonObject jsonObject)
        {
            if (!Key.TryEvaluate(jsonObject, out var property) || property is not JsonValue value)
            {
                return false;
            }

            var valueAsString = value.ToString();
            return Values.Contains(valueAsString, StringComparer.OrdinalIgnoreCase);
    }
    }


    public JsonArray Filter(JsonArray participants)
    {
        var result = new JsonArray(NodeOptions);
        foreach (var jsonNode in participants)
        {
            if (jsonNode is not JsonObject participant)
                continue;

            if (_filters.All(filter => filter.IsMatch(participant)))
            {
                result.Add(participant.DeepClone());
            }
        }

        return result;
    }
}

public static class ParticipantHelper
{
    public static JsonArray Filter(JsonArray rawParticipants, string filter)
    {
        var dynamicArray = rawParticipants.Deserialize<dynamic>()!;
        
        var filteredParticipants = dynamicArray
            .AsQueryable()
            .Where(filter)
            .OfType<JsonNode>()
            .ToJsonArray();
        
        return filteredParticipants;
        
    }
}