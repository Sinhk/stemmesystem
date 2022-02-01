using Duende.IdentityServer.Extensions;
using Microsoft.AspNetCore.SignalR;
using Stemmesystem.Shared;
using Stemmesystem.Shared.SignalR;

namespace Stemmesystem.Server.Hubs;

public class DelegatHub : Hub<IDelegatHubClient>
{
    public async Task SendMessage(string user, string message)
    {
        await Clients.All.ReceiveMessage(user, message);
    }


    public override async Task OnConnectedAsync()
    {
        if (Context.User?.IsAuthenticated() == true)
        {
            if (Context.User.IsInRole("Delegat"))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "Delegat");
                var arrangement = Context.User.FindFirst(AuthConstants.ArrangementClaimType);
                if (arrangement != null) 
                    await Groups.AddToGroupAsync(Context.ConnectionId, arrangement.Value);
            }
            if (Context.User.IsInRole("admin")) 
                await Groups.AddToGroupAsync(Context.ConnectionId, "admin");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (Context.User?.IsAuthenticated() == true)
        {
            if (Context.User.IsInRole("Delegat"))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Delegat");
                var arrangement = Context.User.FindFirst(AuthConstants.ArrangementClaimType);
                if (arrangement != null) 
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, arrangement.Value);
            }
            if (Context.User.IsInRole("admin")) 
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, "admin");
        }
        await base.OnDisconnectedAsync(exception);
    }
}

