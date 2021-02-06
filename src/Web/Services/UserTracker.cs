using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Stemmesystem.Web.Services
{
    public class ActiveTracker
    {
        private readonly ConcurrentDictionary<int, ActiveArrangement> _arrangement;

        public ActiveTracker()
        {
            _arrangement = new ConcurrentDictionary<int, ActiveArrangement>();
        }
        
        public event Action<ActiveTrackerEvent>? CountChanged;

        public async Task<Guid?> RegisterActive(int arrangement, int delegatId)
        {
            var delegater = _arrangement.GetOrAdd(arrangement, i => new ActiveArrangement(arrangement));

            try
            {
                await delegater.Lock.WaitAsync(2000);
                try
                {
                    var delegat = new ActiveDelegat(delegatId);
                    delegater.Delegater.Add(delegat);    
                    CountChanged?.Invoke(new ActiveTrackerEvent(arrangement, delegater.Delegater.Distinct().Count()));
                    return delegat.ActiveKey;
                }
                finally
                {
                    delegater.Lock.Release();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return null;
        }
        public async Task RegisterInactive(int arrangement, Guid key)
        {
            var delegater = _arrangement.GetOrAdd(arrangement, i => new ActiveArrangement(arrangement));
            await delegater.Lock.WaitAsync();
            try
            {
                delegater.Delegater.RemoveAll(d => d.ActiveKey == key);
                CountChanged?.Invoke(new ActiveTrackerEvent(arrangement, delegater.Delegater.Distinct().Count()));
            }
            finally
            {
                delegater.Lock.Release();
            }
        }

        public async Task<int> ActiveCount(int arrangementId)
        {
            var delegater = _arrangement.GetOrAdd(arrangementId, i => new ActiveArrangement(arrangementId));
            await delegater.Lock.WaitAsync();
            try
            {
                return delegater.Delegater.Select(d=> d.DelegatId).Distinct().Count();
            }
            finally
            {
                delegater.Lock.Release();
            }
        }

        public async Task<bool> IsActive(int arrangementId, int delgat)
        {
            var delegater = _arrangement.GetOrAdd(arrangementId, i =>new ActiveArrangement(arrangementId));
            await delegater.Lock.WaitAsync();
            try
            {
                return delegater.Delegater.Any(d=> d.DelegatId == delgat);
            }
            finally
            {
                delegater.Lock.Release();
            }
        }

        public async Task<ReadOnlyCollection<ActiveDelegat>> GetList(int arrangementId)
        {
            var delegater = _arrangement.GetOrAdd(arrangementId, i =>new ActiveArrangement(arrangementId));
            await delegater.Lock.WaitAsync();
            try
            {
                return delegater.Delegater.AsReadOnly();
            }
            finally
            {
                delegater.Lock.Release();
            }
        }
    }

    public record ActiveDelegat(int DelegatId)
    {
        public Guid ActiveKey { get; init; } = Guid.NewGuid();
    }

    internal class ActiveArrangement
    {
        public int ArrangementId { get; }

        public ActiveArrangement(int arrangementId)
        {
            ArrangementId = arrangementId;
        }

        public SemaphoreSlim Lock { get; } = new(1);
        public List<ActiveDelegat> Delegater { get; } = new();
    }

    public record ActiveTrackerEvent(int ArrangementId,int NewCount);
}