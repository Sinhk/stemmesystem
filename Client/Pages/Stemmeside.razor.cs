using System.Collections.Concurrent;
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
        private readonly ConcurrentDictionary<int, VoteringDto> _voteringer = new();
        private IDelegatNotifierService Notifier => Service;

        private bool _disposed;

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
            var voteringer = await ArrangementService.FinnAktiveVoteringer(new ArrangementRequest {ArrangementId = _arrangement.Id});
            foreach (var votering in voteringer)
            {
                _voteringer[votering.Id] = votering;
            }

            _startetSubscription = Notifier.OnVoteringStartet(VoteringStartet);
            _stoppetSubscription = Notifier.OnVoteringStoppet(VoteringStoppet);
            await Notifier.Start();
        }
        
        public void Dispose()
        { 
            if (_disposed)
                return;
            _startetSubscription?.Dispose();
            _stoppetSubscription?.Dispose();
            _disposed = true;
        }

        private void VoteringStartet(VoteringStartetEvent e)
        {
            _voteringer[e.Votering.Id] = e.Votering;
            StateHasChanged();
        }

        private void VoteringStoppet(VoteringStoppetEvent e)
        {
            _voteringer.Remove(e.VoteringId, out _);
            StateHasChanged();
        }
    }
}