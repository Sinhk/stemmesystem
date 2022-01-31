using AutoMapper;
using AutoMapper.QueryableExtensions;
using LazyCache;
using Microsoft.EntityFrameworkCore;
using StemmeSystem.Data;
using Stemmesystem.Server.Data;
using Stemmesystem.Server.Data.Entities;
using Stemmesystem.Shared;
using Stemmesystem.Shared.Interfaces;
using Stemmesystem.Shared.Models;

namespace Stemmesystem.Server.Services
{
    public class ArrangementService : IArrangementService
    {
        private readonly StemmesystemContext _context;

        private readonly IMapper _mapper;

        private readonly IAppCache _cache;

        public ArrangementService(StemmesystemContext context, IMapper mapper, IAppCache cache)
        {
            _mapper = mapper;
            _cache = cache;
            _context = context;
        }

        public async Task<ArrangementDto?> HentArrangementAsync(HentArrangementRequest request, CancellationToken cancellationToken = default)
        {
            if (request.Id != null)
                return await HentArrangementAsync(request.Id.Value, cancellationToken);
            if (request.Navn != null)
                return await HentArrangementAsync(request.Navn, cancellationToken);
            throw new StemmeException("request må ha Id eller navn");
        }

        public async Task<ArrangementDto?> HentArrangementAsync(string navn, CancellationToken cancellationToken = default)
        {
            return await GetSingleQuery(_context)
                .Where(a => a.Navn == navn)
                .ProjectTo<ArrangementDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<ArrangementDto> NyttArrangement(ArrangementInputModel model)
        {
            if (!await IsNameAvailable(model.Navn!))
                throw new StemmeException($"Det finnes allered et arrangement med navn {model.Navn}");
            var entity = _mapper.Map<Arrangement>(model);
            _context.Arrangement.Add(entity);
            await _context.SaveChangesAsync();
            return _mapper.Map<ArrangementDto>(entity);
        }

        public async Task<bool> IsNameAvailable(string name)
        {
            return await _context.Arrangement.AllAsync(a => a.Navn != name);
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

        public async Task<List<ArrangementInfo>> HentArrangementerAsync(CancellationToken cancellationToken = default)
        {
                        return await _context.Arrangement
                .AsSplitQuery()
                .Include(a=> a.Delegater)
                .Include(a=> a.Saker)
                .ThenInclude(s => s.Voteringer)
                .Where(a=> a.Aktiv == true)
                .ProjectTo<ArrangementInfo>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }

        public async Task<ArrangementDto?> HentArrangementAsync(int id, CancellationToken cancellationToken = default)
        {
            var arr = await _cache.GetOrAddAsync($"Arrangement({id})", async () =>
            {
                                var arrangement = await GetSingleQuery(_context)
                    .Where(a => a.Id == id)
                    .ProjectTo<ArrangementDto>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync(cancellationToken);
                return arrangement;
            },  DateTimeOffset.Now.AddSeconds(15));
            return arr;
        }

        public async Task<ArrangementDto> OppdaterArrangement(ArrangementInputModel model)
        {
            var arrangement = await _context.Arrangement
                .Where(a => a.Id == model.Id)
                .FirstOrDefaultAsync();
            if (arrangement == null)
                throw new StemmeException($"Fant ingen arrangement med id {model.Id} å oppdatere");

            arrangement.Beskrivelse = model.Beskrivelse;
            arrangement.Startdato = model.Startdato;
            arrangement.Sluttdato = model.Sluttdato;
            
            await _context.SaveChangesAsync();
            return (await HentArrangementAsync(arrangement.Id))!;
        }

        public async Task<List<VoteringDto>> FinnAktiveVoteringer(ArrangementRequest request)
        {
            return await _context.Arrangement
                .AsSplitQuery()
                .Where(a=> a.Id == request.ArrangementId)
                .SelectMany(a => a.Saker)
                .SelectMany(s => s.Voteringer)
                .Where(v => v.Aktiv)
                .ProjectTo<VoteringDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<List<VoteringResultatDto>> HentResultater(ArrangementRequest request)
        {
            return await _context.Arrangement
                .Where(a=> a.Id == request.ArrangementId)
                .SelectMany(a => a.Saker)
                .SelectMany(s => s.Voteringer)
                .Where(v => v.Publisert)
                .ProjectTo<VoteringResultatDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<ArrangementInfo?> HentArrangementInfoAsync(ArrangementRequest request)
        {
            return await _cache.GetOrAddAsync($"ArrangementInfo({request.ArrangementId})", async () =>
            {
                return await _context.Arrangement
                    .Where(a=> a.Id == request.ArrangementId)
                    .ProjectTo<ArrangementInfo>(_mapper.ConfigurationProvider)
                    .SingleOrDefaultAsync();
            },  DateTimeOffset.Now.AddSeconds(60));
        }
    }
}
