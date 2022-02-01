namespace Stemmesystem.Shared.SignalR;

public interface IDelegatHubClient
{
    Task ReceiveMessage(string user, string message);
    Task VoteringStartet(VoteringStartetEvent e, CancellationToken cancellationToken = default);
    Task VoteringStoppet(VoteringStoppetEvent e, CancellationToken cancellationToken = default);
    Task NyStemme(NyStemmeEvent e, CancellationToken cancellationToken = default);
    Task StemmeFjernet(StemmeFjernetEvent e, CancellationToken cancellationToken = default);
    Task VoteringLukket(VoteringLukketEvent e, CancellationToken cancellationToken = default);
    Task VoteringPublisert(VoteringPublisertEvent e, CancellationToken cancellationToken = default);
    Task NyVotering(NyVoteringEvent e, CancellationToken cancellationToken = default);
    Task HarStemt(HarStemtEvent e, CancellationToken cancellationToken = default);
}