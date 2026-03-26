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
        private DelegateDto? _delegate;
        private readonly ConcurrentDictionary<int, BallotDto> _ballots = new();
        private IDelegateNotifierService Notifier => Service;

        private bool _disposed;

        private IDisposable? _startedSubscription;
        private IDisposable? _stoppedSubscription;

        protected override async Task OnInitializedAsync()
        {
            var result = await DelegateService.GetDelegateInfo();
            _delegate = result.Delegate;
            
            ArrangementInfo? arrangement;
            if(Id != null)
            {
                arrangement = await ArrangementService.GetArrangementInfoAsync(new ArrangementRequest {ArrangementId = Id.Value});
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
            var ballots = await ArrangementService.GetActiveBallots(new ArrangementRequest {ArrangementId = _arrangement.Id});
            foreach (var ballot in ballots)
            {
                _ballots[ballot.Id] = ballot;
            }

            _startedSubscription = Notifier.OnBallotStarted(BallotStarted);
            _stoppedSubscription = Notifier.OnBallotStopped(BallotStopped);
            await Notifier.Start();
        }
        
        public void Dispose()
        { 
            if (_disposed)
                return;
            _startedSubscription?.Dispose();
            _stoppedSubscription?.Dispose();
            _disposed = true;
        }

        private void BallotStarted(BallotStartedEvent e)
        {
            _ballots[e.Ballot.Id] = e.Ballot;
            StateHasChanged();
        }

        private void BallotStopped(BallotStoppedEvent e)
        {
            _ballots.Remove(e.BallotId, out _);
            StateHasChanged();
        }
    }
}