using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;
using Google.Protobuf.WellKnownTypes;
using Google.Rpc;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Stemmesystem.Data;
using Stemmesystem.Data.Entities;
using Stemmesystem.Server.Data.Entities;
using Stemmesystem.Shared.Interfaces;
using Stemmesystem.Shared.MinSpeiding;
using Status = Google.Rpc.Status;

namespace Stemmesystem.Server.Features.MinSpeiding;

[Authorize]
public class MinSpeidingOptionsRepository : IMinSpeidingOptionsRepository
{
    private readonly StemmesystemContext _dbContext;
    private readonly MinSpeidingService _speidingService;

    public MinSpeidingOptionsRepository(StemmesystemContext dbContext, MinSpeidingService speidingService)
    {
        _dbContext = dbContext;
        _speidingService = speidingService;
    }
    public async Task<MinSpeidingOptionsDto> GetMinSpeidingOptions(GetMinSpeidingOptionsRequest request, CancellationToken cancellationToken = default)
    {
        var options = await _dbContext.Arrangement.AsNoTracking()
            .Where(a => a.Id == request.ArrangementId)
            .Select(a => a.MinSpeidingOptions!)
            .SingleOrDefaultAsync(cancellationToken: cancellationToken);

        return options?.ToDto() ?? MinSpeidingOptions.Default.ToDto();
    }

    public async Task<MinSpeidingOptionsDto> SetMinSpeidingId(SetMinSpeidingIdRequest request, CancellationToken cancellationToken = default)
    {
        var arrangement = await Find(request.ArrangementId, cancellationToken);

        arrangement.MinSpeidingOptions ??= new MinSpeidingOptions();
        arrangement.MinSpeidingOptions.MinSpeidingId = request.MinSpeidingId;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return arrangement.MinSpeidingOptions.ToDto();
    }

    public async Task SetMembersApiKey(SetMembersApiKeyRequest request, CancellationToken cancellationToken = default)
    {
        if (request.MembersApiKey?.All(c => c == '*') == true)
            return; // probably accidental sending of placeholder
        
        var arrangement = await Find(request.ArrangementId, cancellationToken);
        
        arrangement.MinSpeidingOptions ??= new MinSpeidingOptions();
        arrangement.MinSpeidingOptions.MembersApiKey = request.MembersApiKey;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task SetImportCheckIn(SetImportCheckInRequest request, CancellationToken cancellationToken = default)
    {
        var arrangement = await Find(request.ArrangementId, cancellationToken);
        
        arrangement.MinSpeidingOptions ??= new MinSpeidingOptions();
        arrangement.MinSpeidingOptions.ImportCheckIn = request.ImportCheckIn;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task SetFilter(SetFilterRequest request, CancellationToken cancellationToken = default)
    {
        var arrangement = await Find(request.ArrangementId, cancellationToken);
        
        arrangement.MinSpeidingOptions ??= new MinSpeidingOptions();
        arrangement.MinSpeidingOptions.Filter = new ParticipantFilter(request.Filter);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<RunImportResult> RunImport(RunImportRequest request, CancellationToken cancellationToken = default)
    {
        var arrangement = await Find(request.ArrangementId, cancellationToken);
        if (arrangement.MinSpeidingOptions is null)
        {
            ThrowError(Code.FailedPrecondition, "Min speiding ikke konfigurert for arrangementet");
        }

        await _dbContext.Entry(arrangement)
            .Collection(a => a.Delegater)
            .LoadAsync(cancellationToken);
        
        var participants = await GetParticipants(arrangement.MinSpeidingOptions);
        var delegates = participants.ToDelegates(arrangement.MinSpeidingOptions.ImportCheckIn);
        
        var existing = arrangement.Delegater
            .Where(d => d.MemberId.HasValue)
            .ToDictionary(d => d.MemberId!.Value);

        var updated = 0;
        var added = 0;
        var delegatnummer = arrangement.Delegater.Max(d => d.Delegatnummer);
        foreach (var delegat in delegates)
        {
            if (existing.TryGetValue(delegat.MemberId!.Value, out var existingDelegat))
            {
                existingDelegat.Navn = delegat.Navn;
                existingDelegat.Epost ??= delegat.Epost;
                existingDelegat.Telefon ??= delegat.Telefon;
                existing.Remove(delegat.MemberId!.Value);
                updated++;
            }
            else
            {
                delegat.Delegatnummer = ++delegatnummer;
                arrangement.Delegater.Add(delegat);
                added++;
            }
        }

        var deleted = 0;
        foreach (var missing in existing.Values)
        {
            //TODO: Should we delete delegates that are no longer in MinSpeiding?
        }
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        return new RunImportResult(added, updated, deleted);
    }

    private async Task<IReadOnlyCollection<Participant>> GetParticipants(MinSpeidingOptions options)
    {
        var result = await _speidingService.GetArrangementParticipants(options.MinSpeidingId, options.MembersApiKey);
        var participants =  result.Match<JsonArray?>(raw =>
        {
            if (raw is null)
            {
                return null;
            }
            return options.Filter is not null ? options.Filter.Filter(raw) : raw;
        }, err => throw CreateError(Code.Internal, err.Message));

        if (participants is null) ThrowError(Code.Internal, "Fikk 'null' ved henting av deltakere");

        return participants.Deserialize<IReadOnlyCollection<Participant>>()!;
    }

    private async Task<Arrangement> Find(int arrangementId, CancellationToken cancellationToken = default)
    {
        var arrangement = await _dbContext.Arrangement.FindAsync(new object[] { arrangementId }, cancellationToken: cancellationToken);
        if (arrangement == null)
        {
            ThrowError(Code.NotFound, "Arrangement ble ikke funnet");
        }

        return arrangement;
    }

    [DoesNotReturn]
    private static void ThrowError(Code errorCode, string message)
    {
        var status = CreateError(errorCode, message);
        throw status;
    }

    private static RpcException CreateError(Code errorCode, string message)
    {
        var status = new Status
        {
            Code = (int)errorCode,
            Details =
            {
                Any.Pack(new BadRequest
                {
                    FieldViolations =
                    {
                        new BadRequest.Types.FieldViolation { Description = message }
                    }
                })
            }
        };
        return status.ToRpcException();
    }
}