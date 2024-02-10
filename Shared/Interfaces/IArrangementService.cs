using ProtoBuf;
using ProtoBuf.Grpc.Configuration;
using Stemmesystem.Core.Models;

[assembly: CompatibilityLevel(CompatibilityLevel.Level300)]

namespace Stemmesystem.Core.Interfaces;

[Service]
public interface IArrangementService
{
    Task<ArrangementDto?> HentArrangementAsync(HentArrangementRequest request, CancellationToken cancellationToken = default);
    IAsyncEnumerable<ArrangementInfo> HentArrangementerAsync(CancellationToken cancellationToken = default);
    Task<ArrangementInfo?> HentArrangementInfoAsync(ArrangementRequest request);
    IAsyncEnumerable<VoteringResultatDto> HentResultater(ArrangementRequest request);
    IAsyncEnumerable<VoteringDto> FinnAktiveVoteringer(ArrangementRequest request);


    Task<ArrangementDto> NyttArrangement(ArrangementInputModel input);
    Task<ArrangementDto> OppdaterArrangement(ArrangementInputModel input);
}

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