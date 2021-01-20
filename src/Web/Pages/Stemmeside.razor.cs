using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Stemmesystem.Web.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Stemmesystem.Web.Data;
using Stemmesystem.Data;

namespace Stemmesystem.Web.Pages
{
    public partial class Stemmeside : IStemmeClient
    {
        [CascadingParameter]
        private DelegatStateProvider? DelegatStateProvider {get;set;}

        [Parameter]
        public string? Navn{get; set;}

        [Parameter]
        public int? Id{ get; set; }


        private Arrangement? _arrangement;
        private Delegat? delegat;
        private HubConnection? _hubConnection;
        protected override async Task OnInitializedAsync()
        {
            if (DelegatStateProvider != null)
            {
                delegat = await DelegatStateProvider.GetDelegat();
            }
            if(Id != null)
            {
                _arrangement = await ArrangementService.HentArrangementAsync(Id.Value);
                Navn = _arrangement.Navn;
            }
            else if(Navn != null)
            {
                _arrangement = await ArrangementService.HentArrangementAsync(Navn);
                Id = _arrangement.Id;
            }
            else
            {
                //ERROR
                NavigationManager.NavigateTo("");
                return;
            }

            if (_arrangement == null)
            {
                NavigationManager.NavigateTo("");
                return;
            }

            _hubConnection = new HubConnectionBuilder().WithUrl(NavigationManager.ToAbsoluteUri("/stemme-hub")).WithAutomaticReconnect().Build();
            _hubConnection.On<NyStemmeEvent>(nameof(IStemmeClient.NyStemme), NyStemme);
            _hubConnection.On<VoteringStartetEvent>(nameof(IStemmeClient.VoteringStartet), VoteringStartet);
            _hubConnection.On<VoteringStoppetEvent>(nameof(IStemmeClient.VoteringStoppet), VoteringStoppet);
            await _hubConnection.StartAsync();
            await _hubConnection.SendAsync(nameof(StemmeHub.BliMedIArrangement), Navn);
        }

        private async Task SetDelegat(Delegat delegat)
        {
            this.delegat = delegat;
            if (DelegatStateProvider != null)
            {
                DelegatStateProvider.Delegatkode = delegat.Delegatkode;
                await DelegatStateProvider.SaveChangesAsync();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_hubConnection != null)
                await _hubConnection.DisposeAsync();
        }

        public async Task NyStemme(NyStemmeEvent e)
        {
            //TODO: Partial update based on event
            /*if (_arrangement == null) return;
            var votering = _arrangement.FinnVotering(stemme.VoteringId);
            
            ArrangementService.
            votering.Stemmer.Add()
            */
            _arrangement = await ArrangementService.HentArrangementAsync(Id!.Value);
            StateHasChanged();
        }

        public async Task VoteringStartet(VoteringStartetEvent e)
        {
            //TODO: Partial update based on event
            System.Console.WriteLine($"Startet votering{e.VoteringId}");
            _arrangement = await ArrangementService.HentArrangementAsync(Id!.Value);
            StateHasChanged();
        }

        public async Task VoteringStoppet(VoteringStoppetEvent e)
        {
            //TODO: Partial update based on event
            System.Console.WriteLine($"Stoppet votering{e.VoteringId}");
            _arrangement = await ArrangementService.HentArrangementAsync(Id!.Value);
            StateHasChanged();
        }
    }
}