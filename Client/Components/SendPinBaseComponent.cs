#nullable disable
using Microsoft.AspNetCore.Components;
using Stemmesystem.Core.Interfaces;

namespace Stemmesystem.Client.Components
{
    public abstract class SendPinBaseComponent : ComponentBase, IDisposable
    {
        [Inject] protected NavigationManager Navigation { get; set; }
        
        [Inject] protected IPinSender PinSender { get; set; }
        protected SendState State { get; private set; } = SendState.NotSent;
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

        public void Dispose()
        {
            _cancelation?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}