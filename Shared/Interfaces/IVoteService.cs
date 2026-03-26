using ProtoBuf.Grpc;
using ProtoBuf.Grpc.Configuration;
using Stemmesystem.Shared.Models;

namespace Stemmesystem.Shared.Interfaces;

[Service]
public interface IVoteService
{
    Task<List<VoteDto>> CastVoteAsync(CastVoteRequest request, CallContext context = default);

    public Task<HasVotedResult> HasVoted(VoteRequest request, CallContext context = default);
}

public record VoteRequest(int BallotId);
public record CastVoteRequest(int BallotId, IEnumerable<Guid> ChoiceIds);
public record HasVotedResult(bool Voted, List<VoteDto>? Votes);
public record AdminBallotRequest(int ArrangementId, int BallotId);

[Service]
public interface IAdminVoteService
{
    Task<GetResult<AdminBallotDto>> StartBallot(AdminBallotRequest request, CancellationToken cancellationToken = default);
    Task<GetResult<AdminBallotDto>> StopBallot(AdminBallotRequest request, CancellationToken cancellationToken = default);
    Task<GetResult<AdminBallotDto>> PublishBallot(AdminBallotRequest request, CancellationToken cancellationToken = default);
    Task<GetResult<AdminBallotDto>> LockBallot(AdminBallotRequest request, CancellationToken cancellationToken = default);
    Task<BallotInputModel> CopyBallot(AdminBallotRequest request, CancellationToken cancellationToken = default);
}