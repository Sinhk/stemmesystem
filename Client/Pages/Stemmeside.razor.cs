using Microsoft.AspNetCore.Components;
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
        private ICollection<VoteringDto> _voteringer = new List<VoteringDto>();
        private NotifierService? _notifier;
        private bool _disposed;

        protected override async Task OnInitializedAsync()
        {
            var result = await DelegatService.HentDelegatInfo();
            Console.WriteLine(result);
            _delegat = result.Delegat;
            
            ArrangementInfo? arrangement;
            if(Id != null)
            {
                arrangement = await ArrangementService.HentArrangementInfoAsync(new ArrangementRequest {ArrangementId = Id.Value});
                Console.WriteLine(arrangement);
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
            _notifier.VoteringStartet += VoteringStartet;
            _notifier.VoteringStoppet += VoteringStoppet;
            */
            
            _voteringer = await ArrangementService.FinnAktiveVoteringer(new ArrangementRequest {ArrangementId = _arrangement.Id});
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

        public void VoteringStartet(VoteringStartetEvent e)
        {
            _ = InvokeAsync(async () =>
            {
                var votering = await SakService.HentVotering(new HentVoteringRequest(e.SakId, e.VoteringId));
                if(votering != null)
                {
                    _voteringer.Add(votering);
                    StateHasChanged();
                }
            });
        }

        public void VoteringStoppet(VoteringStoppetEvent e)
        {
            _ = InvokeAsync(async () =>
            {
                var votering = _voteringer.FirstOrDefault(v => v.Id == e.VoteringId);
                if(votering != null)
                    _voteringer.Remove(votering);
                else
                {
                    if (_arrangement != null)
                        _voteringer = await ArrangementService.FinnAktiveVoteringer(new ArrangementRequest {ArrangementId = _arrangement.Id});
                }
                StateHasChanged();
            });
        }
    }
}