namespace Stemmesystem.Shared.SignalR;

public interface IDelegateHubClient
{
    Task BallotStarted(BallotStartedEvent e, CancellationToken cancellationToken = default);
    Task BallotStopped(BallotStoppedEvent e, CancellationToken cancellationToken = default);
    Task BallotPublished(BallotPublishedEvent e, CancellationToken cancellationToken = default);
    Task ActiveCountChanged(ActiveCountChangedEvent e, CancellationToken cancellationToken = default);
}


public interface IAdminHubClient : IDelegateHubClient
{
    Task NewVote(NewVoteEvent e, CancellationToken cancellationToken = default);
    Task VoteRemoved(VoteRemovedEvent e, CancellationToken cancellationToken = default);
    Task BallotLocked(BallotLockedEvent e, CancellationToken cancellationToken = default);
    Task NewBallot(NewBallotEvent e, CancellationToken cancellationToken = default);
    Task Voted(VotedEvent e, CancellationToken cancellationToken = default);
    Task PresentCountChanged(PresentCountChangedEvent e, CancellationToken cancellationToken = default);
}