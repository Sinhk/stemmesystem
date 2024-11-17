using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Stemmesystem.Client.SignalR;
using Stemmesystem.Shared;
using Stemmesystem.Shared.Interfaces;
using Stemmesystem.Shared.Models;

namespace Stemmesystem.Client.Pages
{
    public partial class Resultatside
    {
        [CascadingParameter]
        private Task<AuthenticationState> AuthenticationStateTask { get; set; } = null!;
        
        private bool _disposed;
        private List<ArrangementInfo>? _arrangementer;
        private IDisposable? _subscription;
        private bool _nyPublisert;

        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthenticationStateTask;
            
            if (authState.User.Identity?.IsAuthenticated == false)
                return;
            if (authState.User.IsInRole("admin"))
            {
                var arrangement = await _arrangementService.HentArrangementerAsync();
                _arrangementer = arrangement;

                var notifier = ScopedServices.GetRequiredService<IAdminNotifierService>();
                _subscription = notifier.OnVoteringPublisert(OnVoteringPublisert);
                await notifier.Start();
            }
            else if (authState.User.IsInRole("Delegat"))
            {
                var arrangementClaim = authState.User.FindFirst(AuthConstants.ArrangementClaimType);
                if (arrangementClaim?.Value != null)
                {
                    var id = int.Parse(arrangementClaim.Value);
                    var arrangement = await _arrangementService.HentArrangementInfoAsync(new ArrangementRequest {ArrangementId = id});
                    if (arrangement != null) _arrangementer = new List<ArrangementInfo> { arrangement };
                    var notifier = ScopedServices.GetRequiredService<IDelegatNotifierService>();
                    _subscription = notifier.OnVoteringPublisert(OnVoteringPublisert);
                    await notifier.Start();
                }
            }
        }
        
        private void OnVoteringPublisert(VoteringPublisertEvent e)
        {
            _nyPublisert = true;
            StateHasChanged();
        }

        public void Dispose() => Dispose(true);

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            _subscription?.Dispose();
            _disposed = true;
        }
    }
}