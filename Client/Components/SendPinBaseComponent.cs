using Microsoft.AspNetCore.Components;
using Stemmesystem.Core.Interfaces;

namespace Stemmesystem.Client.Components
{
    public abstract class SendPinBaseComponent : ComponentBase, IDisposable
    {
        [Inject] protected NavigationManager Navigation { get; set; } = default!;
        
        [Inject] protected IPinSender PinSender { get; set; } = default!;
        protected SendState State { get; private set; } = SendState.NotSent;
        private CancellationTokenSource? _cancellation;
        
        [Parameter]
        public EventCallback OnSent { get; set; }
        
        protected async Task SendPin()
        {
            if (_cancellation != null)
            {
                await _cancellation.CancelAsync();
            }
            State = SendState.Sending;
            StateHasChanged();
            var success = await DoSend();
            State = success 
                ? SendState.Sent 
                : SendState.Failed;
            if (success)
                await OnSent.InvokeAsync();

            StateHasChanged();
            _cancellation = new CancellationTokenSource();
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
            _cancellation?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}