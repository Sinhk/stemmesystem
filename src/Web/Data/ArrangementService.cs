using Microsoft.EntityFrameworkCore;
using Stemmesystem.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Stemmesystem.Web.Data
{
    public class ArrangementService
    {
        private readonly StemmesystemContext context;

        public ArrangementService(StemmesystemContext context)
        {
            this.context = context;
        }

        public async Task<Arrangement> HentArrangementAsync(string navn, CancellationToken cancellationToken = default)
        {
            return await GetSingleQuery()
                .Where(a => a.Navn == navn)
                .FirstOrDefaultAsync(cancellationToken);
        }

        private IQueryable<Arrangement> GetSingleQuery()
        {
            return context.Arrangement
                            .AsSingleQuery()
                            .Include(a => a.Delegater)
                            .Include(a => a.Saker)
                            .ThenInclude(s => s.Voteringer)
                            .ThenInclude(s=> s.AvgitStemme)
                            .Where(a => a.Aktiv == true)
                            ;
        }

        public async Task<ICollection<Arrangement>> HentArrangementAsync(CancellationToken cancellationToken = default)
        {
            return await context.Arrangement
                .AsSplitQuery()
                .Include(a=> a.Delegater)
                .Include(a=> a.Saker)
                .ThenInclude(s => s.Voteringer)
                .Where(a=> a.Aktiv == true)
                .ToListAsync(cancellationToken);
        }

        public async Task<Arrangement> HentArrangementAsync(int id, CancellationToken cancellationToken = default)
        {
            return await GetSingleQuery()
                .Where(a => a.Id == id)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
