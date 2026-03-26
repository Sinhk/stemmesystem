using Duende.IdentityServer.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Stemmesystem.Shared.SignalR;

namespace Stemmesystem.Server.Hubs;

[Authorize(Roles = "admin")]
public class AdminHub : Hub<IAdminHubClient>
{
    public async Task JoinArrangement(int arrangementId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, arrangementId.ToString());
    }
    public async Task LeaveArrangement(int arrangementId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, arrangementId.ToString());
    }
    
    public override async Task OnConnectedAsync()
    {
        if (Context.User?.IsAuthenticated() == true)
        {
            if (Context.User.IsInRole("admin")) 
                await Groups.AddToGroupAsync(Context.ConnectionId, "admin");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (Context.User?.IsAuthenticated() == true)
        {
            if (Context.User.IsInRole("admin")) 
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, "admin");
        }
        await base.OnDisconnectedAsync(exception);
    }
}