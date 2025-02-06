using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Stemmesystem.Shared.Interfaces;
using Stemmesystem.Shared.MinSpeiding;

namespace Stemmesystem.Client.Components;

public partial class MinSpeidingImport
{
    private MinSpeidingOptionsModel? _minSpeidingOptions;
    private JsonArray? _rawParticipants;
    private IReadOnlyCollection<string> _participantProperties = Array.Empty<string>();
    private IReadOnlyCollection<Participant>? _participants;
    private string? _participantError;
    private bool _rawResultExpanded;
    private RunImportResult? _importResult;
    [Parameter] public int ArrangementId { get; set; }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var minSpeidingOptions =
                await Repository.GetMinSpeidingOptions(new GetMinSpeidingOptionsRequest(ArrangementId));
            _minSpeidingOptions = MinSpeidingOptionsModel.FromDto(minSpeidingOptions);
            StateHasChanged();
        }
    }

    private async Task HandleValidSubmit(EditContext arg)
    {
        if (_minSpeidingOptions is null)
        {
            return;
        }

        if (arg.IsModified(() => _minSpeidingOptions.Id!) && _minSpeidingOptions.Id.HasValue)
        {
            await Repository.SetMinSpeidingId(new SetMinSpeidingIdRequest(ArrangementId, _minSpeidingOptions.Id.Value));
        }

        if (arg.IsModified(() => _minSpeidingOptions.MembersApiKey!))
        {
            await Repository.SetMembersApiKey(new SetMembersApiKeyRequest(ArrangementId,
                _minSpeidingOptions.MembersApiKey));
        }

        if (arg.IsModified(() => _minSpeidingOptions.ImportCheckIn))
        {
            await Repository.SetImportCheckIn(new SetImportCheckInRequest(ArrangementId,
                _minSpeidingOptions.ImportCheckIn));
        }

        if (arg.IsModified(() => _minSpeidingOptions.Filter!) && _minSpeidingOptions.Filter is not null)
        {
            await Repository.SetFilter(new SetFilterRequest(ArrangementId, _minSpeidingOptions.Filter));
        }

        arg.MarkAsUnmodified();
    }

    private async Task LoadParticipants(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await HttpClient.GetAsync($"arrangement/{ArrangementId}/participants", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _participantError = $"Feil ved henting av deltakere: {await response.Content.ReadAsStringAsync(cancellationToken)}";
                return;
            }
            _rawParticipants = await response.Content.ReadFromJsonAsync<JsonArray>(cancellationToken: cancellationToken);
            Logger.LogInformation("Hentet {Count} deltakere", _rawParticipants!.Count);
        }
        catch (Exception e)
        {
            _participantError = $"Ukjent feil ved henting av deltakere: {e.Message}";
            return;
        }

        if (_minSpeidingOptions?.Filter is not null)
        {
            FilterParticipants();
        }
        else
        {
            var participants = _rawParticipants.Deserialize<IReadOnlyCollection<Participant>>();
            if (participants is null)
            {
                _participantError = "Kunne ikke hente deltakere";
                return;
            }

            _participants = participants;
            _participantProperties = ExtractParticipantProperties(_rawParticipants);
        }
    }

    private void FilterParticipants()
    {
        _rawResultExpanded = false;
        var filter = _minSpeidingOptions?.Filter;
        if (string.IsNullOrEmpty(filter))
        {
            _participantError = "Filteret kan ikke være tomt";
            return;
        }

        if (_rawParticipants is null)
        {
            _participantError = "Ingen deltakere å filtrere";
            return;
        }

        var participantFilter = new ParticipantFilter(filter);

        var filteredParticipants = participantFilter.Filter(_rawParticipants);

        var participants = filteredParticipants.Deserialize<IReadOnlyCollection<Participant>>();
        if (participants is null)
        {
            _participantError = "Ukjent feil ved filtrering";
            return;
        }

        _participants = participants;

        Repository.SetFilter(new SetFilterRequest(ArrangementId, filter));
        Logger.LogInformation("Filtrerte deltakere med filter {Filter}, fikk {Count}", filter, _participants.Count);
    }

    private static IReadOnlyCollection<string> ExtractParticipantProperties(JsonArray? participants)
    {
        if (participants == null)
            return Array.Empty<string>();

        var props = new HashSet<string>();
        foreach (var participant in participants)
        {
            if (participant is not JsonObject jsonObject) continue;
            foreach (var property in jsonObject)
            {
                props.Add(property.Key);
            }
        }

        return props;
    }

    public class MinSpeidingOptionsModel
    {
        public int? Id { get; set; }
        public string? MembersApiKey { get; set; }
        public bool ImportCheckIn { get; set; }
        public string? Filter { get; set; }

        public MinSpeidingOptionsModel(int? id, string? membersApiKey, bool importCheckIn, string? filter)
        {
            Id = id;
            MembersApiKey = membersApiKey;
            ImportCheckIn = importCheckIn;
            Filter = filter;
        }

        public static MinSpeidingOptionsModel FromDto(MinSpeidingOptionsDto dto)
        {
            return new MinSpeidingOptionsModel(dto.MinSpeidingId, dto.MembersApiKeySet ? "********" : null,
                dto.ImportCheckIn, dto.Filter);
        }
    }

    private async Task Import()
    {
        _importResult = await Repository.RunImport(new RunImportRequest(ArrangementId));
    }
}