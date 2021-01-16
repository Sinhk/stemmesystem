using Microsoft.AspNetCore.SignalR;
using Stemmesystem.Data;
using System.Threading.Tasks;

namespace Stemmesystem.Web.Data
{
    public class StemmeHub : Hub<IStemmeClient>
    {
        public async Task BliMedIArrangement(string arrangement)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, arrangement);
        }
    }

    public interface IStemmeClient
    {
        Task NyStemme(NyStemmeEvent stemmEvent);
        Task VoteringStartet(VoteringStartetEvent stemmEvent);
        Task VoteringStoppet(VoteringStoppetEvent stemmEvent);
    }
}
