#nullable disable
using Microsoft.AspNetCore.Components;
using Stemmesystem.Shared.Interfaces;

namespace Stemmesystem.Client.Components
{
    public abstract class SendPinBaseComponent : ComponentBase
    {
        [Inject] protected NavigationManager Navigation { get; set; }
        
        [Inject] protected IPinSender PinSender { get; set; }
        protected SendState State = SendState.NotSent;
        
        [Parameter]
        public EventCallback OnSendt { get; set; }
        
        protected async Task SendPin()
        {
            State = SendState.Sending;
            StateHasChanged();
            var success = await DoSend();
            State = success 
                ? SendState.Sent 
                : SendState.Failed;
            if (success)
                await OnSendt.InvokeAsync();

            StateHasChanged();
            
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