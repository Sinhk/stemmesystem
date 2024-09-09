using ProtoBuf.Grpc.Configuration;

namespace Stemmesystem.Shared.Interfaces;

//TODO: redo as one update supporting partial updates
[Service]
public interface IMinSpeidingOptionsRepository
{
    Task<MinSpeidingOptionsDto> GetMinSpeidingOptions(GetMinSpeidingOptionsRequest request, CancellationToken cancellationToken = default);
    Task<MinSpeidingOptionsDto> SetMinSpeidingId(SetMinSpeidingIdRequest request, CancellationToken cancellationToken = default);
    Task SetMembersApiKey(SetMembersApiKeyRequest request, CancellationToken cancellationToken = default);
    Task SetImportCheckIn(SetImportCheckInRequest request, CancellationToken cancellationToken = default);
    Task SetFilter(SetFilterRequest request, CancellationToken cancellationToken = default);
    Task<RunImportResult> RunImport(RunImportRequest request, CancellationToken cancellationToken = default);
}

public record GetMinSpeidingOptionsRequest(int ArrangementId);
public record SetMinSpeidingIdRequest(int ArrangementId, int MinSpeidingId);
public record SetMembersApiKeyRequest(int ArrangementId, string? MembersApiKey);
public record SetImportCheckInRequest(int ArrangementId, bool ImportCheckIn);
public record SetFilterRequest(int ArrangementId, string Filter);
public record RunImportRequest(int ArrangementId);
public record RunImportResult(int Added, int Updated, int Deleted);
public record MinSpeidingOptionsDto(int? MinSpeidingId, bool MembersApiKeySet, bool ImportCheckIn, string? Filter);