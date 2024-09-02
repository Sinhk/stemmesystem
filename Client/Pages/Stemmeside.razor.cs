using Microsoft.AspNetCore.Components;
using Stemmesystem.Client.SignalR;
using Stemmesystem.Shared;
using Stemmesystem.Shared.Interfaces;
using Stemmesystem.Shared.Models;

namespace Stemmesystem.Client.Pages
{
    public partial class Stemmeside
    {
        [Parameter]
        public int? Id{ get; set; }

        private ArrangementInfo? _arrangement;
        private DelegatDto? _delegat;
        private List<VoteringDto> _voteringer = new();
        private IDelegatNotifierService Notifier => Service;

        private bool _disposed;
        private List<IDisposable> _subscriptions = new();
        private IDisposable? _startetSubscription;
        private IDisposable? _stoppetSubscription;

        protected override async Task OnInitializedAsync()
        {
            var result = await DelegatService.HentDelegatInfo();
            _delegat = result.Delegat;
            
            ArrangementInfo? arrangement;
            if(Id != null)
            {
                arrangement = await ArrangementService.HentArrangementInfoAsync(new ArrangementRequest {ArrangementId = Id.Value});
            }
            else
            {
                //ERROR
                NavigationManager.NavigateTo("");
                return;
            }

            if (arrangement == null)
            {
                NavigationManager.NavigateTo("");
                return;
            }

            _arrangement = arrangement;
            /*
             _notifier = Notifications.ForArrangement(_arrangement.Id);
            */
            
            _voteringer = await ArrangementService.FinnAktiveVoteringer(new ArrangementRequest {ArrangementId = _arrangement.Id});

            _startetSubscription = Notifier.OnVoteringStartet(VoteringStartet);
            _stoppetSubscription = Notifier.OnVoteringStoppet(VoteringStoppet);
            await Notifier.Start();
        }
        
        public void Dispose() => Dispose(true);

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            
            _startetSubscription?.Dispose();
            _stoppetSubscription?.Dispose();
            
/*
            if (disposing)
            {
                if (Tracker != null && _arrangement != null &&_delegat != null)
                    Tracker.RegisterInactive(_arrangement.Id, _delegat.Id);
            }
*/
            _disposed = true;
        }

        private void VoteringStartet(VoteringStartetEvent e)
        {
            _voteringer.Add(e.Votering);
                StateHasChanged();
        }

        private void VoteringStoppet(VoteringStoppetEvent e)
        {
            _voteringer.RemoveAll(v => v.Id == e.VoteringId);
            StateHasChanged();
        }
    }
}