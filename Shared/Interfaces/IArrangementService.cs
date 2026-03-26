using ProtoBuf;
using ProtoBuf.Grpc.Configuration;
using Stemmesystem.Shared.Models;

[assembly: CompatibilityLevel(CompatibilityLevel.Level300)]

namespace Stemmesystem.Shared.Interfaces;

[Service]
public interface IArrangementService
{
    Task<ArrangementDto?> GetArrangementAsync(GetArrangementRequest request, CancellationToken cancellationToken = default);
    Task<List<ArrangementInfo>> GetArrangementsAsync(CancellationToken cancellationToken = default);
    Task<ArrangementInfo?> GetArrangementInfoAsync(ArrangementRequest request);
    Task<IReadOnlyCollection<BallotResultDto>> GetResults(ArrangementRequest request,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<BallotDto>> GetActiveBallots(ArrangementRequest request, CancellationToken cancellationToken = default);


    Task<ArrangementDto> CreateArrangement(ArrangementInputModel dto);
    Task<ArrangementDto> UpdateArrangement(ArrangementInputModel dto);
    Task<PresentCountResponse> GetPresentCount(PresentCountRequest request, CancellationToken cancellationToken = default);
}

public record PresentCountRequest(int ArrangementId);
public record PresentCountResponse(int Count);

[ProtoContract]
public record GetArrangementRequest
{
    [ProtoMember(1)]
    
    public int? Id { get; init; }
    [ProtoMember(2)]
    public string? Name { get; init; }
}

[ProtoContract]
public record ArrangementRequest
{
    [ProtoMember(1, IsRequired = true)]
    public int ArrangementId { get; init; }
}

[ProtoContract(SkipConstructor = true)]
public record GetActiveBallotsRequest
{
    [ProtoMember(1, IsRequired = true)]
    public int ArrangementId { get; init; }
}