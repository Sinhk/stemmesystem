using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Stemmesystem.Web.Services
{
    public class ActiveTracker
    {
        private readonly ConcurrentDictionary<int, List<ActiveDelegat>> _arrangement;

        public ActiveTracker()
        {
            _arrangement = new ConcurrentDictionary<int, List<ActiveDelegat>>();
        }
        
        public event Action<ActiveTrackerEvent>? CountChanged;

        public void RegisterActive(int arrangement, int delegat)
        {
            var delegater = _arrangement.GetOrAdd(arrangement, i => new List<ActiveDelegat>());

            lock (delegater)
            {
                delegater.Add(new ActiveDelegat(delegat));    
            }

            CountChanged?.Invoke(new ActiveTrackerEvent(arrangement, delegater.Distinct().Count()));
        }
        public void RegisterInactive(int arrangement, int delegat)
        {
            var delegater = _arrangement.GetOrAdd(arrangement, i => new List<ActiveDelegat>());

            lock (delegater)
            {
                delegater.Remove(new ActiveDelegat(delegat)); //Works because it's a record, with one property
            }
            
            CountChanged?.Invoke(new ActiveTrackerEvent(arrangement, delegater.Distinct().Count()));
        }

        public int ActiveCount(int arrangementId)
        {
            var delegater = _arrangement.GetOrAdd(arrangementId, i => new List<ActiveDelegat>());
            lock (delegater)
            {
                return delegater.Distinct().Count();
            }
        }

        public bool IsActive(int arrangementId, int delgat)
        {
            var delegater = _arrangement.GetOrAdd(arrangementId, i => new List<ActiveDelegat>());
            lock (delegater)
            {
                return delegater.Any(d=> d.DelegatId == delgat);
            }
        }
    }

    internal record ActiveDelegat(int DelegatId);

    public record ActiveTrackerEvent(int ArrangementId,int NewCount);
}