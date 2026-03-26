using Stemmesystem.Shared.Models;

namespace Stemmesystem.Shared;

  
public class NotifierService
{
    public int ArrangementId { get; }

    public NotifierService(int arrangementId)
    {
        ArrangementId = arrangementId;
    }

    public void OnBallotStarted(BallotStartedEvent e) => BallotStarted?.Invoke(e);
    public void OnBallotStopped(BallotStoppedEvent e) => BallotStopped?.Invoke(e);
    public void OnNewVote(NewVoteEvent e) => NewVote?.Invoke(e);
    public void OnVoteRemoved(VoteRemovedEvent e) => VoteRemoved?.Invoke(e);

    public void OnBallotLocked(BallotLockedEvent e) => BallotLocked?.Invoke(e);
    public void OnBallotPublished(BallotPublishedEvent e) => BallotPublished?.Invoke(e);

    public void OnNewBallot(NewBallotEvent e) => NewBallot?.Invoke(e);

    public void OnVoted(VotedEvent e) => Voted?.Invoke(e);
    public event Action<BallotStartedEvent>? BallotStarted;

    public event Action<BallotStoppedEvent>? BallotStopped;

    public event Action<NewVoteEvent>? NewVote;

    public event Action<VoteRemovedEvent>? VoteRemoved;

    public event Action<BallotLockedEvent>? BallotLocked;

    public event Action<BallotPublishedEvent>? BallotPublished;
    public event Action<NewBallotEvent>? NewBallot;
    public event Action<VotedEvent>? Voted;
}


public record NewVoteEvent(int BallotId, VoteDto Vote);

public record VoteRemovedEvent(int BallotId, Guid VoteId);

public record BallotStartedEvent(BallotDto Ballot);

public record BallotStoppedEvent(int BallotId, DateTime Time, IReadOnlyCollection<VoteDto> Votes);

public record BallotLockedEvent(int CaseId, int BallotId, int? DelegatesPresent);

public record BallotPublishedEvent(int ArrangementId, int CaseId, int BallotId);

public record ActiveCountChangedEvent(int ArrangementId, int NewCount);


public record NewBallotEvent(AdminBallotDto Ballot);

public record VotedEvent(int BallotId, int DelegateId);
public record PresentCountChangedEvent(int ArrangementId, int Count);