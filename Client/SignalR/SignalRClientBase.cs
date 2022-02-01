using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.SignalR.Client;

namespace Stemmesystem.Client.SignalR;

public abstract class SignalRClientBase : ISignalRClient, IAsyncDisposable
{
    protected bool Started { get; private set; }

    protected SignalRClientBase(NavigationManager navigationManager, IAccessTokenProvider tokenProvider, string hubPath) =>
        HubConnection = new HubConnectionBuilder()
            .WithUrl(navigationManager.ToAbsoluteUri(hubPath), options =>
            {
                options.AccessTokenProvider = async () =>
                {
                    var result = await tokenProvider.RequestAccessToken();
                    result.TryGetToken(out var token);
                    return token?.Value;
                };
            })
            .WithAutomaticReconnect()
            .Build();

    public bool IsConnected => HubConnection?.State == HubConnectionState.Connected;

    protected HubConnection HubConnection { get; private set; }

    public async ValueTask DisposeAsync()
    {
        await HubConnection.DisposeAsync();
    }

    public async Task Start()
    {
        if (!Started)
        {
            await HubConnection.StartAsync();
            Started = true;
        }
    }
}