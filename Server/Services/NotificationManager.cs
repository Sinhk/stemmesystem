using Microsoft.AspNetCore.SignalR;
using Stemmesystem.Server.Hubs;
using Stemmesystem.Shared;
using Stemmesystem.Shared.SignalR;

namespace Stemmesystem.Server.Services;

public class NotificationManager
{

    private readonly IHubContext<DelegateHub, IDelegateHubClient> _delegateContext;
    private readonly IHubContext<AdminHub, IAdminHubClient> _adminContext;

    public NotificationManager(IHubContext<DelegateHub, IDelegateHubClient> delegateContext, IHubContext<AdminHub, IAdminHubClient> adminContext)
    {
        _delegateContext = delegateContext;
        _adminContext = adminContext;
    }

    public IDelegateHubClient ForArrangement(int arrangementId)
    {
        var client = _delegateContext.Clients.Group(arrangementId.ToString());
        return client;
    }

    public IAdminHubClient ForAdmin(int? arrangementId = null)
    {
        if (arrangementId != null)
            return _adminContext.Clients.Group(arrangementId.Value.ToString());
        return _adminContext.Clients.All;
    }
}