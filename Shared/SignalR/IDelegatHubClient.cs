namespace Stemmesystem.Shared.SignalR;

public interface IDelegatHubClient
{
    Task VoteringStartet(VoteringStartetEvent e, CancellationToken cancellationToken = default);
    Task VoteringStoppet(VoteringStoppetEvent e, CancellationToken cancellationToken = default);
    Task VoteringPublisert(VoteringPublisertEvent e, CancellationToken cancellationToken = default);
    Task ActiveCountChanged(ActiveCountChangedEvent e, CancellationToken cancellationToken = default);
}


public interface IAdminHubClient : IDelegatHubClient
{
    Task NyStemme(NyStemmeEvent e, CancellationToken cancellationToken = default);
    Task StemmeFjernet(StemmeFjernetEvent e, CancellationToken cancellationToken = default);
    Task VoteringLukket(VoteringLukketEvent e, CancellationToken cancellationToken = default);
    Task NyVotering(NyVoteringEvent e, CancellationToken cancellationToken = default);
    Task VoteringSlettet(VoteringSlettetEvent e, CancellationToken cancellationToken = default);
    Task NySak(NySakEvent e, CancellationToken cancellationToken = default);
    Task SakSlettet(SakSlettetEvent e, CancellationToken cancellationToken = default);
    Task HarStemt(HarStemtEvent e, CancellationToken cancellationToken = default);
    Task TilstedeCountChanged(TilstedeCountChangedEvent e, CancellationToken cancellationToken = default);
}