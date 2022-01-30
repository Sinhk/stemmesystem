using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Stemmesystem.Shared;
using Stemmesystem.Shared.Interfaces;
using Stemmesystem.Shared.Models;

namespace Stemmesystem.Client.Pages
{
    public partial class Resultatside
    {
        [CascadingParameter]
        private Task<AuthenticationState> AuthenticationStateTask { get; set; } = null!;
        
        private NotifierService? _notifier;
        private bool _disposed;
        private List<ArrangementInfo>? _arrangementer;

        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthenticationStateTask;
            
            if (authState.User.Identity?.IsAuthenticated == false)
                return;
            if (authState.User.IsInRole("admin"))
            {
                var arrangement = await _arrangementService.HentArrangementerAsync();
                _arrangementer = arrangement;
            }
            else if (authState.User.IsInRole("Delegat"))
            {
                var arrangementClaim = authState.User.FindFirst(AuthConstants.ArrangementClaimType);
                if (arrangementClaim?.Value != null)
                {
                    var id = int.Parse(arrangementClaim.Value);
                    var arrangement = await _arrangementService.HentArrangementInfoAsync(new ArrangementRequest {ArrangementId = id});
                    if (arrangement != null) _arrangementer = new List<ArrangementInfo> { arrangement };
                }
            }
            
            /*
             _notifier = Notifications.ForArrangement(_arrangement.Id);
            _notifier.VoteringStartet += VoteringStartet;
            _notifier.VoteringStoppet += VoteringStoppet;
            */
        }
        
        public void Dispose() => Dispose(true);

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;
/*
            if (_notifier != null)
            {
                _notifier.VoteringStartet -= VoteringStartet;
                _notifier.VoteringStoppet -= VoteringStoppet;
            }
            */
/*
            if (disposing)
            {
                if (Tracker != null && _arrangement != null &&_delegat != null)
                    Tracker.RegisterInactive(_arrangement.Id, _delegat.Id);
            }
*/
            _disposed = true;
        }
    }
}