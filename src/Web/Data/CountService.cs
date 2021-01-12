using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Stemmesystem.Web.Data
{
    public class CountService
    {
        private int _count;

        public CountService()
        {
        }

        public int GetCurrentCount()
        {
            return _count;
        }

        public void IncrementCount()
        {
            _count++;

        }
    }

    public class CountHub : Hub
    {
        private readonly CountService countService;

        public CountHub(CountService countService)
        {
            this.countService = countService;
        }

        public async Task SendUpdate()
        {
            countService.IncrementCount();
            await Clients.All.SendAsync("ReceiveMessage", countService.GetCurrentCount());
        }
    }
}
