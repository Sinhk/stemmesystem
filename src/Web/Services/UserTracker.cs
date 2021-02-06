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

        public async Task RegisterActive(int arrangement, int delegat)
        {
            var delegater = _arrangement.GetOrAdd(arrangement, i => new ActiveArrangement(arrangement));

            await delegater.Lock.WaitAsync();
            try
            {
                delegater.Delegater.Add(new ActiveDelegat(delegat));    
                CountChanged?.Invoke(new ActiveTrackerEvent(arrangement, delegater.Delegater.Distinct().Count()));
            }
            finally
            {
                delegater.Lock.Release();
            }
        }
        public async Task RegisterInactive(int arrangement, int delegat)
        {
            var delegater = _arrangement.GetOrAdd(arrangement, i => new ActiveArrangement(arrangement));
            await delegater.Lock.WaitAsync();
            try
            {
                delegater.Delegater.Remove(new ActiveDelegat(delegat)); //Works because it's a record, with one property
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
                return delegater.Delegater.Distinct().Count();
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

    public record ActiveDelegat(int DelegatId);

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