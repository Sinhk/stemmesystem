using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.SignalR.Client;
using Stemmesystem.Shared;
using Stemmesystem.Shared.SignalR;

namespace Stemmesystem.Client.SignalR;

public interface IDelegatNotifierService : ISignalRClient
{
    IDisposable? OnVoteringStartet(Action<VoteringStartetEvent> action);
    IDisposable? OnVoteringStoppet(Action<VoteringStoppetEvent> action);
}

public interface IAdminNotifierService : ISignalRClient
{
    Task KobleTilArrangement(int arrangementId, CancellationToken cancellationToken = default);
    Task KobleFraArrangement(int arrangementId, CancellationToken cancellationToken = default);
    IDisposable? OnVoteringStartet(Action<VoteringStartetEvent> action);
    IDisposable? OnVoteringStoppet(Action<VoteringStoppetEvent> action);
    IDisposable? OnNyStemme(Action<NyStemmeEvent> action);
    IDisposable? OnStemmeFjernet(Action<StemmeFjernetEvent> action);
    IDisposable? OnVoteringLukket(Action<VoteringLukketEvent> action);
    IDisposable? OnVoteringPublisert(Action<VoteringPublisertEvent> action);
    IDisposable? OnNyVotering(Action<NyVoteringEvent> action);
    IDisposable? OnHarStemt(Action<HarStemtEvent> action);
}

public class DelegatNotifierService : SignalRClientBase, IDelegatNotifierService
{
    public DelegatNotifierService(NavigationManager navigationManager, IAccessTokenProvider tokenProvider) : base(navigationManager, tokenProvider, "hubs/delegat")
    {
    }

    public IDisposable OnVoteringStartet(Action<VoteringStartetEvent> action)
        => HubConnection.On(nameof(IDelegatHubClient.VoteringStartet), action);

    public IDisposable OnVoteringStoppet(Action<VoteringStoppetEvent> action)
        => HubConnection.On(nameof(IDelegatHubClient.VoteringStoppet), action);
}

public class AdminNotifierService : SignalRClientBase, IAdminNotifierService
{
    public AdminNotifierService(NavigationManager navigationManager, IAccessTokenProvider tokenProvider) : base(navigationManager, tokenProvider, "hubs/admin")
    {
    }

    public async Task KobleTilArrangement(int arrangementId, CancellationToken cancellationToken = default)
    {
        try
        {
            await HubConnection.InvokeAsync("KobleTilArrangement", arrangementId, cancellationToken);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Console.WriteLine(e.Message);
        }
    }

    public async Task KobleFraArrangement(int arrangementId, CancellationToken cancellationToken = default)
    {
        try
        {
            await HubConnection.InvokeAsync("KobleFraArrangement", arrangementId, cancellationToken);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Console.WriteLine(e.Message);
        }
    }

    public IDisposable OnVoteringStartet(Action<VoteringStartetEvent> action) => HubConnection.On(nameof(IDelegatHubClient.VoteringStartet), action);

    public IDisposable OnVoteringStoppet(Action<VoteringStoppetEvent> action) => HubConnection.On(nameof(IDelegatHubClient.VoteringStoppet), action);

    public IDisposable OnNyStemme(Action<NyStemmeEvent> action) => HubConnection.On(nameof(IAdminHubClient.NyStemme), action);

    public IDisposable OnStemmeFjernet(Action<StemmeFjernetEvent> action) => HubConnection.On(nameof(IAdminHubClient.StemmeFjernet), action);

    public IDisposable OnVoteringLukket(Action<VoteringLukketEvent> action) => HubConnection.On(nameof(IAdminHubClient.VoteringLukket), action);

    public IDisposable OnVoteringPublisert(Action<VoteringPublisertEvent> action) => HubConnection.On(nameof(IAdminHubClient.VoteringPublisert), action);

    public IDisposable OnNyVotering(Action<NyVoteringEvent> action) => HubConnection.On(nameof(IAdminHubClient.NyVotering), action);

    public IDisposable OnHarStemt(Action<HarStemtEvent> action) => HubConnection.On(nameof(IAdminHubClient.HarStemt), action);
}