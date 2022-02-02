using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using Stemmesystem.Server.Hubs;
using Stemmesystem.Shared;
using Stemmesystem.Shared.SignalR;

namespace Stemmesystem.Server.Services;

public class NotificationManager
{

    private readonly IHubContext<DelegatHub, IDelegatHubClient> _delegatContext;
    private readonly IHubContext<AdminHub, IAdminHubClient> _adminContext;

    public NotificationManager(IHubContext<DelegatHub, IDelegatHubClient> delegatContext, IHubContext<AdminHub, IAdminHubClient> adminContext)
    {
        _delegatContext = delegatContext;
        _adminContext = adminContext;
    }

    public IDelegatHubClient ForArrangement(int arrangementId)
    {
        var client = _delegatContext.Clients.Group(arrangementId.ToString());
        return client;
    }

    public IAdminHubClient ForAdmin(int? arrangementId = null)
    {
        if (arrangementId != null)
            return _adminContext.Clients.Group(arrangementId.Value.ToString());
        return _adminContext.Clients.All;
    }
}