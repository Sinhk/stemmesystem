using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.SignalR.Client;
using Stemmesystem.Shared;
using Stemmesystem.Shared.SignalR;

namespace Stemmesystem.Client.SignalR;

public interface IDelegatSignalRClient : IDelegatHubClient, ISignalRClient
{
    
}

public interface INotifierService : ISignalRClient
{
    void OnVoteringStartet(Action<VoteringStartetEvent> action);
    void OnVoteringStoppet(Action<VoteringStoppetEvent> action);
    void OnNyStemme(Action<NyStemmeEvent> action);
    void OnStemmeFjernet(Action<StemmeFjernetEvent> action);
    void OnVoteringLukket(Action<VoteringLukketEvent> action);
    void OnVoteringPublisert(Action<VoteringPublisertEvent> action);
    void OnNyVotering(Action<NyVoteringEvent> action);
    void OnHarStemt(Action<HarStemtEvent> action);
}

public class NotifierService : SignalRClientBase, INotifierService
{
    public NotifierService(NavigationManager navigationManager, IAccessTokenProvider tokenProvider) : base(navigationManager, tokenProvider, "hubs/delegat")
    {
    }

    public void OnVoteringStartet(Action<VoteringStartetEvent> action)
    {
        if (!Started)
        {
            HubConnection.On(nameof(IDelegatHubClient.VoteringStartet), action);
        }
    }

    public void OnVoteringStoppet(Action<VoteringStoppetEvent> action)
    {
        if (!Started)
        {
            HubConnection.On(nameof(IDelegatHubClient.VoteringStoppet), action);
        }
    }

    public void OnNyStemme(Action<NyStemmeEvent> action)
    {
        if (!Started)
        {
            HubConnection.On(nameof(IDelegatHubClient.NyStemme), action);
        }
    }

    public void OnStemmeFjernet(Action<StemmeFjernetEvent> action)
    {
        if (!Started)
        {
            HubConnection.On(nameof(IDelegatHubClient.StemmeFjernet), action);
        }
    }

    public void OnVoteringLukket(Action<VoteringLukketEvent> action)
    {
        if (!Started)
        {
            HubConnection.On(nameof(IDelegatHubClient.VoteringLukket), action);
        }
    }

    public void OnVoteringPublisert(Action<VoteringPublisertEvent> action)
    {
        if (!Started)
        {
            HubConnection.On(nameof(IDelegatHubClient.VoteringPublisert), action);
        }
    }

    public void OnNyVotering(Action<NyVoteringEvent> action)
    {
        if (!Started)
        {
            HubConnection.On(nameof(IDelegatHubClient.NyVotering), action);
        }
    }

    public void OnHarStemt(Action<HarStemtEvent> action)
    {
        if (!Started)
        {
            HubConnection.On(nameof(IDelegatHubClient.HarStemt), action);
        }
    }
}
