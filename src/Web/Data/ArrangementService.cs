using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Stemmesystem.Data;
using Stemmesystem.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using LazyCache;

namespace Stemmesystem.Web.Data
{
    public class ArrangementService
    {
        private readonly IDbContextFactory<StemmesystemContext> _contextFactory;
        private readonly IMapper _mapper;
        private readonly IAppCache _cache;

        public ArrangementService(IDbContextFactory<StemmesystemContext> contextFactory, IMapper mapper, IAppCache cache)
        {
            _contextFactory = contextFactory;
            _mapper = mapper;
            _cache = cache;
        }
        public async Task<Arrangement?> HentArrangementAsync(string navn, CancellationToken cancellationToken = default)
        {
            await using var context = _contextFactory.CreateDbContext();
            return await GetSingleQuery(context)
                .Where(a => a.Navn == navn)
                .FirstOrDefaultAsync(cancellationToken);
        }
        public async Task<ArrangementModel> NyttArrangement(ArrangementModel model)
        {
            await using var context = _contextFactory.CreateDbContext();
            if (!await IsNameAvailable(model.Navn!))
                throw new StemmeException($"Det finnes allered et arrangement med navn {model.Navn}");
            var entity = _mapper.Map<Arrangement>(model);
            context.Arrangement.Add(entity);
            await context.SaveChangesAsync();
            return _mapper.Map<ArrangementModel>(entity);
        }

        public async Task<bool> IsNameAvailable(string name)
        {
            await using var context = _contextFactory.CreateDbContext();
            return await context.Arrangement.AllAsync(a => a.Navn != name);
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

        public async Task<ICollection<Arrangement>> HentArrangementAsync(CancellationToken cancellationToken = default)
        {
            await using var context = _contextFactory.CreateDbContext();
            return await context.Arrangement
                .AsSplitQuery()
                .Include(a=> a.Delegater)
                .Include(a=> a.Saker)
                .ThenInclude(s => s.Voteringer)
                .Where(a=> a.Aktiv == true)
                .ToListAsync(cancellationToken);
        }

        public Task<Arrangement?> HentArrangementAsync(int id, CancellationToken cancellationToken = default)
        {
            return _cache.GetOrAddAsync($"Arrangement({id})", async () =>
            {
                await using var context = _contextFactory.CreateDbContext();
                var arrangement = await GetSingleQuery(context)
                    .Where(a => a.Id == id)
                    .FirstOrDefaultAsync();
                return arrangement;
            },  DateTimeOffset.Now.AddSeconds(15));

        }
        
        public async Task<ArrangementModel?> HentArrangementModelAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var context = _contextFactory.CreateDbContext();
            return await GetSingleQuery(context)
                .Where(a => a.Id == id)
                .ProjectTo<ArrangementModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<ArrangementModel> OppdaterArrangement(ArrangementModel model)
        {
            await using var context = _contextFactory.CreateDbContext();
            var arrangement = await context.Arrangement
                .Where(a => a.Id == model.Id)
                .FirstOrDefaultAsync();
            if (arrangement == null)
                throw new StemmeException($"Fant ingen arrangement med id {model.Id} å oppdatere");

            arrangement.Beskrivelse = model.Beskrivelse;
            arrangement.Startdato = model.Startdato;
            arrangement.Sluttdato = model.Sluttdato;
            
            await context.SaveChangesAsync();
            return _mapper.Map<ArrangementModel>(model);
        }

        public async Task<ICollection<Votering>> FinnAktiveVoteringer(int arrangementId)
        {
            await using var context = _contextFactory.CreateDbContext();
            return await context.Arrangement
                .SelectMany(a => a.Saker)
                .SelectMany(s => s.Voteringer)
                .Where(v => v.Aktiv)
                .ToListAsync();
        }

        public async Task<ArrangementInfo?> HentArrangementInfoAsync(int arrangementId)
        {
            return await _cache.GetOrAddAsync($"ArrangementInof({arrangementId})", async () =>
            {
                await using var context = _contextFactory.CreateDbContext();
                return await context.Arrangement
                    .ProjectTo<ArrangementInfo>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync();
            },  DateTimeOffset.Now.AddSeconds(60));
        }
    }

    public record ArrangementInfo
    {
        public int Id { get; init; }
        public string Navn { get; init; } = null!;
    }
}
