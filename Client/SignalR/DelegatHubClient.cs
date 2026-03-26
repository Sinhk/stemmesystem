using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.SignalR.Client;
using Stemmesystem.Shared;
using Stemmesystem.Shared.SignalR;

namespace Stemmesystem.Client.SignalR;

public interface IDelegateNotifierService : ISignalRClient
{
    IDisposable? OnBallotStarted(Action<BallotStartedEvent> action);
    IDisposable? OnBallotStopped(Action<BallotStoppedEvent> action);
    IDisposable? OnBallotPublished(Action<BallotPublishedEvent> action);
    IDisposable OnActiveCountChanged(Action<ActiveCountChangedEvent> action);
    Task<int> GetActiveCount(int arrangementId, CancellationToken cancellationToken = default);
}

public interface IAdminNotifierService : ISignalRClient
{
    Task JoinArrangement(int arrangementId, CancellationToken cancellationToken = default);
    Task LeaveArrangement(int arrangementId, CancellationToken cancellationToken = default);
    IDisposable? OnBallotStarted(Action<BallotStartedEvent> action);
    IDisposable? OnBallotStopped(Action<BallotStoppedEvent> action);
    IDisposable? OnNewVote(Action<NewVoteEvent> action);
    IDisposable? OnVoteRemoved(Action<VoteRemovedEvent> action);
    IDisposable? OnBallotLocked(Action<BallotLockedEvent> action);
    IDisposable? OnBallotPublished(Action<BallotPublishedEvent> action);
    IDisposable? OnNewBallot(Action<NewBallotEvent> action);
    IDisposable? OnVoted(Action<VotedEvent> action);

    IDisposable OnPresentCountChanged(Action<PresentCountChangedEvent> action);
}

public class DelegateNotifierService : SignalRClientBase, IDelegateNotifierService
{
    public DelegateNotifierService(NavigationManager navigationManager, IAccessTokenProvider tokenProvider) : base(navigationManager, tokenProvider, "hubs/delegat")
    {
    }
    public async Task<int> GetActiveCount(int arrangementId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await HubConnection.InvokeAsync<int>("GetActiveCount", arrangementId, cancellationToken);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Console.WriteLine(e.Message);
        }

        return 0;
    }

    public IDisposable OnBallotStarted(Action<BallotStartedEvent> action)
        => HubConnection.On(nameof(IDelegateHubClient.BallotStarted), action);

    public IDisposable OnBallotStopped(Action<BallotStoppedEvent> action)
        => HubConnection.On(nameof(IDelegateHubClient.BallotStopped), action);
    public IDisposable OnBallotPublished(Action<BallotPublishedEvent> action)
        => HubConnection.On(nameof(IDelegateHubClient.BallotPublished), action);
    public IDisposable OnActiveCountChanged(Action<ActiveCountChangedEvent> action)
        => HubConnection.On(nameof(IDelegateHubClient.ActiveCountChanged), action);
}

public class AdminNotifierService : SignalRClientBase, IAdminNotifierService
{
    public AdminNotifierService(NavigationManager navigationManager, IAccessTokenProvider tokenProvider) : base(navigationManager, tokenProvider, "hubs/admin")
    {
    }

    public async Task JoinArrangement(int arrangementId, CancellationToken cancellationToken = default)
    {
        try
        {
            await HubConnection.InvokeAsync("JoinArrangement", arrangementId, cancellationToken);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Console.WriteLine(e.Message);
        }
    }

    public async Task LeaveArrangement(int arrangementId, CancellationToken cancellationToken = default)
    {
        try
        {
            await HubConnection.InvokeAsync("LeaveArrangement", arrangementId, cancellationToken);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Console.WriteLine(e.Message);
        }
    }

    public IDisposable OnBallotStarted(Action<BallotStartedEvent> action) => HubConnection.On(nameof(IDelegateHubClient.BallotStarted), action);

    public IDisposable OnBallotStopped(Action<BallotStoppedEvent> action) => HubConnection.On(nameof(IDelegateHubClient.BallotStopped), action);

    public IDisposable OnNewVote(Action<NewVoteEvent> action) => HubConnection.On(nameof(IAdminHubClient.NewVote), action);

    public IDisposable OnVoteRemoved(Action<VoteRemovedEvent> action) => HubConnection.On(nameof(IAdminHubClient.VoteRemoved), action);

    public IDisposable OnBallotLocked(Action<BallotLockedEvent> action) => HubConnection.On(nameof(IAdminHubClient.BallotLocked), action);

    public IDisposable OnBallotPublished(Action<BallotPublishedEvent> action) => HubConnection.On(nameof(IDelegateHubClient.BallotPublished), action);

    public IDisposable OnNewBallot(Action<NewBallotEvent> action) => HubConnection.On(nameof(IAdminHubClient.NewBallot), action);

    public IDisposable OnVoted(Action<VotedEvent> action) => HubConnection.On(nameof(IAdminHubClient.Voted), action);

    public IDisposable OnPresentCountChanged(Action<PresentCountChangedEvent> action) => HubConnection.On(nameof(IAdminHubClient.PresentCountChanged), action);
}