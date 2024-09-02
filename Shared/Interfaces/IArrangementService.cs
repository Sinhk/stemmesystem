using ProtoBuf;
using ProtoBuf.Grpc.Configuration;
using Stemmesystem.Shared.Models;

[assembly: CompatibilityLevel(CompatibilityLevel.Level300)]

namespace Stemmesystem.Shared.Interfaces;

[Service]
public interface IArrangementService
{
    Task<ArrangementDto?> HentArrangementAsync(HentArrangementRequest request, CancellationToken cancellationToken = default);
    Task<List<ArrangementInfo>> HentArrangementerAsync(CancellationToken cancellationToken = default);
    Task<ArrangementInfo?> HentArrangementInfoAsync(ArrangementRequest request);
    Task<List<VoteringResultatDto>> HentResultater(ArrangementRequest request);
    Task<List<VoteringDto>> FinnAktiveVoteringer(ArrangementRequest request);


    Task<ArrangementDto> NyttArrangement(ArrangementInputModel dto);
    Task<ArrangementDto> OppdaterArrangement(ArrangementInputModel dto);
    Task<TilstedeCountResponse> GetTilstedeCount(TilstedeCountRequest request, CancellationToken cancellationToken = default);
}

public record TilstedeCountRequest(int ArrangementId);
public record TilstedeCountResponse(int Count);

[ProtoContract]
public record HentArrangementRequest
{
    [ProtoMember(1)]
    
    public int? Id { get; init; }
    [ProtoMember(2)]
    public string? Navn { get; init; }
}

[ProtoContract]
public record ArrangementRequest
{
    [ProtoMember(1, IsRequired = true)]
    public int ArrangementId { get; init; }
}

[ProtoContract(SkipConstructor = true)]
public record FinnAktiveVotringerRequest
{
    [ProtoMember(1, IsRequired = true)]
    public int ArrangementId { get; init; }
}