using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using Stemmesystem.Server.Hubs;
using Stemmesystem.Shared;
using Stemmesystem.Shared.SignalR;

namespace Stemmesystem.Server.Services;

public class NotificationManager
{

    private readonly IHubContext<DelegatHub, IDelegatHubClient> _delegatContext;
    
    private readonly IDictionary<int, NotifierService> _notifiers = new ConcurrentDictionary<int, NotifierService>();

    public NotificationManager(IHubContext<DelegatHub, IDelegatHubClient> delegatContext)
    {
        this._delegatContext = delegatContext;
    }

    public IDelegatHubClient ForArrangement(int arrangementId)
    {
        var client = _delegatContext.Clients.Group(arrangementId.ToString());
        return client;
    }
}