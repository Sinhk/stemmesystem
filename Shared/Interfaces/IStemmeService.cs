using ProtoBuf.Grpc;
using ProtoBuf.Grpc.Configuration;
using Stemmesystem.Shared.Models;

namespace Stemmesystem.Shared.Interfaces;

[Service]
public interface IStemmeService
{
    Task<List<StemmeDto>> AvgiStemmeAsync(AvgiStemmeRequest request, CallContext context = default);

    public Task<HarStemmtResult> HarStemmt(StemmeRequest request, CallContext context = default);
    // public Task<List<StemmeDto>> FinnStemmer(int voteringId, string delegatKode, CancellationToken cancellationToken = default);
    //
    // Task<StemmeDto> FinnStemme(int voteringId, Guid stemmeId, CancellationToken cancellationToken = default);
}

public record StemmeRequest(int VoteringId);
public record AvgiStemmeRequest(int VoteringId, IEnumerable<Guid> ValgIder);
public record HarStemmtResult(bool HarStemmt, List<StemmeDto>? Stemmer);
public record AdminStemmeRequest(int ArrangementId, int VoteringId);

[Service]
public interface IAdminStemmeService
{
    Task<HentResult<AdminVoteringDto>> StartVotering(AdminStemmeRequest request, CancellationToken cancellationToken = default);
    Task<HentResult<AdminVoteringDto>> StoppVotering(AdminStemmeRequest request, CancellationToken cancellationToken = default);
    Task<HentResult<AdminVoteringDto>> PubliserVotering(AdminStemmeRequest request, CancellationToken cancellationToken = default);
    Task<HentResult<AdminVoteringDto>> LukkVotering(AdminStemmeRequest request, CancellationToken cancellationToken = default);
    Task<VoteringDto> KopierVotering(AdminStemmeRequest request, CancellationToken cancellationToken = default);
}