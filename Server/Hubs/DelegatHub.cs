using System.Collections.Concurrent;
using Duende.IdentityServer.Extensions;
using Microsoft.AspNetCore.SignalR;
using Stemmesystem.Shared;
using Stemmesystem.Shared.SignalR;

namespace Stemmesystem.Server.Hubs;

public class DelegatHub : Hub<IDelegatHubClient>
{
    private static readonly ConcurrentDictionary<string, int> Tracker = new();
    private static readonly ConcurrentDictionary<int, int> Counts = new();

    public  int GetActiveCount(int arrangementId)
    {
        Counts.TryGetValue(arrangementId, out var i);
        return i;
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
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, arrangement.Value);
                    var x = Tracker.AddOrUpdate(Context.User.GetSubjectId(), _ => 1, (_, i) => i + 1);
                    if (x == 1)
                    {
                        var arrangemntId = int.Parse(arrangement.Value);
                        Counts.AddOrUpdate(arrangemntId, _ => 1, (_, i) => i+1);
                        await CountChanged(arrangemntId);
                    }
                }

                
            }
            if (Context.User.IsInRole("admin")) 
                await Groups.AddToGroupAsync(Context.ConnectionId, "admin");
        }

        await base.OnConnectedAsync();
    }

    private async Task CountChanged(int arrangemntId)
    {
        Console.WriteLine("Count Changed");

        var count = Counts[arrangemntId];
        Console.WriteLine($"count {count}");
        await Clients.Groups("admin").CountChanged(new ActiveCountChangedEvent(arrangemntId,count));
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
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, arrangement.Value);
                    var subjectId = Context.User.GetSubjectId();
                    if (Tracker.TryGetValue(subjectId,out var x) && Tracker.TryUpdate(subjectId, x-1,x))
                    {
                        if (x == 1)
                        {
                            var arrangemntId = int.Parse(arrangement.Value);
                            Counts.AddOrUpdate(arrangemntId, _ => 0, (_, i) => i-1);
                            await CountChanged(arrangemntId);
                        }
                    }
                }

                
            }
            if (Context.User.IsInRole("admin")) 
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, "admin");
        }
        await base.OnDisconnectedAsync(exception);
    }
}