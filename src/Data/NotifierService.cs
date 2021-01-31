using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Stemmesystem.Data
{
    public class NotificationManager
    {
        private readonly IDictionary<int, NotifierService> _notifiers = new ConcurrentDictionary<int, NotifierService>();

        public NotifierService ForArrangement(int arrangementId)
        {
            if (_notifiers.TryGetValue(arrangementId, out var notifier)) return notifier;
            
            notifier = new NotifierService(arrangementId);
            _notifiers[arrangementId] = notifier;
            return notifier;
        }
    }
    
    public class NotifierService
    {
        public int ArrangementId { get; }

        public NotifierService(int arrangementId)
        {
            ArrangementId = arrangementId;
        }

        public void OnVoteringStartet(VoteringStartetEvent e) => VoteringStartet?.Invoke(e);
        public void OnVoteringStoppet(VoteringStoppetEvent e) => VoteringStoppet?.Invoke(e);
        public void OnNyStemme(NyStemmeEvent e) => NyStemme?.Invoke(e);
        public void OnStemmeFjernet(StemmeFjernetEvent e) => StemmeFjernet?.Invoke(e);

        public void OnVoteringLukket(VoteringLukketEvent e) => VoteringLukket?.Invoke(e);
        public void OnVoteringPublisert(VoteringPublisertEvent e) => VoteringPublisert?.Invoke(e);
        public event Action<VoteringStartetEvent>? VoteringStartet;

        public event Action<VoteringStoppetEvent>? VoteringStoppet;

        public event Action<NyStemmeEvent>? NyStemme;

        public event Action<StemmeFjernetEvent>? StemmeFjernet;
        public event Action<VoteringLukketEvent>? VoteringLukket;
        public event Action<VoteringPublisertEvent>? VoteringPublisert;
    }


    public record NyStemmeEvent(int VoteringId, Stemme Stemme);
    public record StemmeFjernetEvent(int VoteringId, Guid StemmeId);
    public record VoteringStartetEvent(int VoteringId);
    public record VoteringStoppetEvent(int VoteringId);
    public record VoteringLukketEvent(int VoteringId);
    public record VoteringPublisertEvent(int VoteringId, Votering Votering);
}