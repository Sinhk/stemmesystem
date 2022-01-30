using Microsoft.EntityFrameworkCore;
using Stemmesystem.Server.Data.Entities;

namespace Stemmesystem.Server.Data.Repositories;

public interface IArrangementRepository
{
    Task<Arrangement?> HentArrangementAsync(int id, CancellationToken cancellationToken = default);
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
    
    private IQueryable<Arrangement> GetSingleQuery(StemmesystemContext context)
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