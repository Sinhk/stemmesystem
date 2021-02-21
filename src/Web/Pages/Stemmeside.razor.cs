using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Stemmesystem.Web.Components;
using Stemmesystem.Data;
using Stemmesystem.Web.Data;

namespace Stemmesystem.Web.Pages
{
    public partial class Stemmeside
    {
        [CascadingParameter]
        private DelegatStateProvider? DelegatStateProvider {get;set;}

        public string? Navn{get; set;}

        [Parameter]
        public int? Id{ get; set; }

        private ArrangementInfo _arrangement = null!;
        private Delegat? _delegat;
        private ICollection<Votering> _voteringer = new List<Votering>();
        private NotifierService? _notifier;
        private bool _resulaterExpanded;
        private Guid? _activeKey;
        private ICollection<Sak> _saker = new List<Sak>();

        protected override async Task OnInitializedAsync()
        {
            if (DelegatStateProvider != null)
            {
                _delegat = await DelegatStateProvider.GetDelegat();
            }

            ArrangementInfo? arrangement;
            if(Id != null)
            {
                arrangement = await ArrangementService.HentArrangementInfoAsync(Id.Value);
                Navn = _arrangement?.Navn;
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
            _notifier = Notifications.ForArrangement(_arrangement.Id);
            _notifier.VoteringStartet += VoteringStartet;
            _notifier.VoteringStoppet += VoteringStoppet;
            _voteringer = await ArrangementService.FinnAktiveVoteringer(_arrangement.Id);
            _saker = await SakService.HentSakerLite(arrangement.Id);
            if (_delegat != null)
            {
                _activeKey = await Tracker.RegisterActive(_arrangement.Id,_delegat.Id);
            }
        }

        private async Task SetDelegat(Delegat delegat)
        {
            _delegat = delegat;
            if (DelegatStateProvider != null)
            {
                DelegatStateProvider.Delegatkode = delegat.Delegatkode;
                await DelegatStateProvider.SaveChangesAsync();
            }
        }

        public void Dispose()
        {
            if (_notifier != null)
            {
                _notifier.VoteringStartet -= VoteringStartet;
                _notifier.VoteringStoppet -= VoteringStoppet;
            }

            if (Tracker != null && _activeKey != null)
            {
                _ = Tracker.RegisterInactive(_arrangement.Id,_activeKey.Value);
            }
        }

        public void VoteringStartet(VoteringStartetEvent e)
        {
            _ = InvokeAsync( () =>
            {
                _voteringer.Add(e.Votering);
                StateHasChanged();
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
                    _voteringer = await ArrangementService.FinnAktiveVoteringer(_arrangement.Id);
                }
                StateHasChanged();
            });
        }
    }
}