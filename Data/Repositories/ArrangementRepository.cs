using Microsoft.EntityFrameworkCore;
using Stemmesystem.Data;
using Stemmesystem.Data.Entities;
using Stemmesystem.Server.Data.Entities;

namespace Stemmesystem.Server.Data.Repositories;

public interface IArrangementRepository
{
    Task<Arrangement?> GetArrangementAsync(int id, CancellationToken cancellationToken = default);
    Task<Ballot?> FindBallotAsync(int arrangementId, int ballotId, CancellationToken cancellationToken = default);
}

public class ArrangementRepository : IArrangementRepository
{
    private readonly StemmesystemContext _context;

    public ArrangementRepository(StemmesystemContext context)
    {
        _context = context;
    }

    public async Task<Arrangement?> GetArrangementAsync(int id, CancellationToken cancellationToken = default)
    {
        var arrangement = await GetSingleQuery(_context)
            .Where(a => a.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
        return arrangement;
    }

    public async Task<Ballot?> FindBallotAsync(int arrangementId, int ballotId, CancellationToken cancellationToken = default)
    {
        return await _context.Arrangements
            .AsTracking()
            .AsSingleQuery()
            .Where(a => a.Id == arrangementId)
            .Where(a => a.Active == true)
            .SelectMany(a => a.Cases)
            .SelectMany(s => s.Ballots)
            .Where(v => v.Id == ballotId)
            .Include(v => v.Votes)
            .SingleOrDefaultAsync(cancellationToken: cancellationToken);
    }

    private IQueryable<Arrangement> GetSingleQuery(StemmesystemContext context)
    {
        return context.Arrangements
                .AsSingleQuery()
                .Include(a => a.Delegates)
                .Include(a => a.Cases)
                .ThenInclude(s => s.Ballots)
                .ThenInclude(s=> s.VotedDelegates)
                .Include(a => a.Cases)
                .ThenInclude(s => s.Ballots)
                .ThenInclude(s => s.Votes)
                .Where(a => a.Active == true)
            ;
    }

}