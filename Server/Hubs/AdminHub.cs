using Duende.IdentityServer.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Stemmesystem.Shared.SignalR;

namespace Stemmesystem.Server.Hubs;

[Authorize(Roles = "admin")]
public class AdminHub : Hub<IAdminHubClient>
{
    public async Task KobleTilArrangement(int arrangementId)
    {
        Console.WriteLine("Adding to group");
        await Groups.AddToGroupAsync(Context.ConnectionId, arrangementId.ToString());
    }
    public async Task KobleFraArrangement(int arrangementId)
    {
        Console.WriteLine("Removing from group");
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, arrangementId.ToString());
    }
    
    public override async Task OnConnectedAsync()
    {
        Console.WriteLine($"{Context.UserIdentifier} Connecting with id {Context.ConnectionId}");
        
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