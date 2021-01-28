using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Stemmesystem.Web.Components;
using Stemmesystem.Data;

namespace Stemmesystem.Web.Pages
{
    public partial class Stemmeside
    {
        [CascadingParameter]
        private DelegatStateProvider? DelegatStateProvider {get;set;}

        [Parameter]
        public string? Navn{get; set;}

        [Parameter]
        public int? Id{ get; set; }

        private Arrangement _arrangement = null!;
        private Delegat? _delegat;
        private ICollection<Votering> _voteringer = new List<Votering>();
        private NotifierService? _notifier;
        private bool _resulaterExpanded;

        protected override async Task OnInitializedAsync()
        {
            if (DelegatStateProvider != null)
            {
                _delegat = await DelegatStateProvider.GetDelegat();
            }

            Arrangement? arrangement;
            if(Id != null)
            {
                arrangement = await ArrangementService.HentArrangementAsync(Id.Value);
                Navn = _arrangement?.Navn;
            }
            else if(Navn != null)
            {
                arrangement = await ArrangementService.HentArrangementAsync(Navn);
                Id = _arrangement?.Id;
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
            _voteringer = _arrangement.AktiveVoteringer().ToList();
        }

        private async Task SetDelegat(Delegat delegat)
        {
            this._delegat = delegat;
            if (DelegatStateProvider != null)
            {
                DelegatStateProvider.Delegatkode = delegat.Delegatkode;
                await DelegatStateProvider.SaveChangesAsync();
            }
        }

        public void Dispose()
        {
            if (_notifier == null) return;
                
            _notifier.VoteringStartet -= VoteringStartet;
            _notifier.VoteringStoppet -= VoteringStoppet;
            
        }

        public void VoteringStartet(VoteringStartetEvent e)
        {
            _ = InvokeAsync(async () =>
            {
                var votering = _arrangement!.FinnVotering(e.VoteringId);
                if(votering != null)
                    _voteringer.Add(votering);
                else
                {
                    _voteringer = await ArrangementService.FinnAktiveVoteringer(_arrangement.Id);
                }
                StateHasChanged();
            });
        }

        public void VoteringStoppet(VoteringStoppetEvent e)
        {
            _ = InvokeAsync(async () =>
            {
                var votering = _arrangement!.FinnVotering(e.VoteringId);
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