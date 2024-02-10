using Microsoft.EntityFrameworkCore;
using StemmeSystem.Data.Entities;
using Stemmesystem.Server.Data.Entities;

namespace StemmeSystem.Data.Repositories;

public interface IArrangementRepository
{
    Task<Arrangement?> HentArrangementAsync(int id, CancellationToken cancellationToken = default);
    Task<Votering?> FinnVoteringAsync(int arrangemntId, int voteringId, CancellationToken cancellationToken = default);
}

public class ArrangementRepository : IArrangementRepository
{
    private readonly StemmesystemContext _context;

    public ArrangementRepository(StemmesystemContext context)
    {
        _context = context;
    }

    public async Task<Arrangement?> HentArrangementAsync(int id, CancellationToken cancellationToken = default)
    {
        var arrangement = await GetSingleQuery(_context)
            .Where(a => a.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
        return arrangement;
    }

    public async Task<Votering?> FinnVoteringAsync(int arrangemntId, int voteringId, CancellationToken cancellationToken = default)
    {
        return await _context.Arrangement
            .AsTracking()
            .AsSingleQuery()
            .Where(a => a.Id == arrangemntId)
            .Where(a => a.Aktiv == true)
            .SelectMany(a => a.Saker)
            .SelectMany(s => s.Voteringer)
            .Where(v => v.Id == voteringId)
            .SingleOrDefaultAsync(cancellationToken: cancellationToken);
    }

    private static IQueryable<Arrangement> GetSingleQuery(StemmesystemContext context)
    {
        return context.Arrangement
                .AsSingleQuery()
                .Include(a => a.Delegater)
                .Include(a => a.Saker)
                .ThenInclude(s => s.Voteringer)
                .ThenInclude(s=> s.AvgitStemme)
                .Include(a => a.Saker)
                .ThenInclude(s => s.Voteringer)
                .ThenInclude(s => s.Stemmer)
                .Where(a => a.Aktiv == true)
            ;
    }

}