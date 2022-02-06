using Stemmesystem.Shared.Models;

namespace Stemmesystem.Shared;

  
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

    public void OnNyVotering(NyVoteringEvent e) => NyVotering?.Invoke(e);

    public void OnHarStemt(HarStemtEvent e) => HarStemt?.Invoke(e);
    public event Action<VoteringStartetEvent>? VoteringStartet;

    public event Action<VoteringStoppetEvent>? VoteringStoppet;

    public event Action<NyStemmeEvent>? NyStemme;

    public event Action<StemmeFjernetEvent>? StemmeFjernet;

    public event Action<VoteringLukketEvent>? VoteringLukket;

    public event Action<VoteringPublisertEvent>? VoteringPublisert;
    public event Action<NyVoteringEvent>? NyVotering;
    public event Action<HarStemtEvent>? HarStemt;
}


public record NyStemmeEvent(int VoteringId, StemmeDto Stemme);

public record StemmeFjernetEvent(int VoteringId, Guid StemmeId);

public record VoteringStartetEvent(VoteringDto Votering);

public record VoteringStoppetEvent(int VoteringId, DateTime Time);

public record VoteringLukketEvent(int SakId, int VoteringId);

public record VoteringPublisertEvent(int ArrangementId, int SakId, int VoteringId);

public record ActiveCountChangedEvent(int ArrangementId, int NewCount);


public record NyVoteringEvent(AdminVoteringDto Votering);

public record HarStemtEvent(int VoteringId, int DelegatId);