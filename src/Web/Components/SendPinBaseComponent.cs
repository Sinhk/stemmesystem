#nullable disable
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Stemmesystem.Web.Services;

namespace Stemmesystem.Web.Components
{
    public abstract class SendPinBaseComponent : ComponentBase
    {
        [Inject] protected NavigationManager Navigation { get; set; }
        
        [Inject] protected IPinSender PinSender { get; set; }
        protected SendState State = SendState.NotSent;
        private CancellationTokenSource _cancelation;
        
        [Parameter]
        public EventCallback OnSendt { get; set; }
        
        protected async Task SendPin()
        {
            _cancelation?.Cancel();
            State = SendState.Sending;
            StateHasChanged();
            var success = await DoSend();
            State = success 
                ? SendState.Sent 
                : SendState.Failed;
            if (success)
                await OnSendt.InvokeAsync();

            StateHasChanged();
            _cancelation = new CancellationTokenSource();
            await Task.Delay(TimeSpan.FromSeconds(1));
            State = SendState.NotSent;
        }

        protected abstract Task<bool> DoSend();

        protected enum SendState
        {
            NotSent,
            Sending,
            Sent,
            Failed
        }
    }
}