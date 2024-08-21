using AutoMapper;
using AutoMapper.QueryableExtensions;
using LazyCache;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Stemmesystem.Data;
using Stemmesystem.Server.Data.Entities;
using Stemmesystem.Core;
using Stemmesystem.Core.Interfaces;
using Stemmesystem.Core.Models;

namespace Stemmesystem.Server.Services
{
    [Authorize]
    public class ArrangementService(StemmesystemContext context, IMapper mapper, IAppCache cache) : IArrangementService
    {
        private readonly StemmesystemContext _context = context;
        private readonly IMapper _mapper = mapper;
        private readonly IAppCache _cache = cache;

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

        public async Task<ArrangementDto> NyttArrangement(ArrangementInputModel input, CancellationToken cancellationToken = default)
        {
            if (!await IsNameAvailable(input.Navn!))
                throw new StemmeException($"Det finnes allered et arrangement med navn {input.Navn}");
            var entity = _mapper.Map<Arrangement>(input);
            _context.Arrangement.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return _mapper.Map<ArrangementDto>(entity);
        }

        public async Task<bool> IsNameAvailable(string name)
        {
            return await _context.Arrangement.AllAsync(a => a.Navn != name);
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

        public IAsyncEnumerable<ArrangementInfo> HentArrangementerAsync(CancellationToken cancellationToken = default)
        {
                return _context.Arrangement
                .AsSplitQuery()
                .Include(a=> a.Delegater)
                .Include(a=> a.Saker)
                .ThenInclude(s => s.Voteringer)
                .Where(a=> a.Aktiv == true)
                .ProjectTo<ArrangementInfo>(_mapper.ConfigurationProvider)
                .AsAsyncEnumerable();
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

        public async Task<ArrangementDto> OppdaterArrangement(ArrangementInputModel input, CancellationToken cancellationToken = default)
        {
            var arrangement = await _context.Arrangement
                .Where(a => a.Id == input.Id)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken);
            if (arrangement == null)
                throw new StemmeException($"Fant ingen arrangement med id {input.Id} å oppdatere");

            arrangement.Beskrivelse = input.Beskrivelse;
            arrangement.Startdato = input.Startdato;
            arrangement.Sluttdato = input.Sluttdato;
            
            await _context.SaveChangesAsync(cancellationToken);
            return (await HentArrangementAsync(arrangement.Id, cancellationToken))!;
        }

        public IAsyncEnumerable<VoteringDto> FinnAktiveVoteringer(ArrangementRequest request)
        {
            return _context.Arrangement
                .AsSplitQuery()
                .Where(a=> a.Id == request.ArrangementId)
                .SelectMany(a => a.Saker)
                .SelectMany(s => s.Voteringer)
                .Where(v => v.Aktiv)
                .ProjectTo<VoteringDto>(_mapper.ConfigurationProvider)
                .AsAsyncEnumerable();
        }

        public IAsyncEnumerable<VoteringResultatDto> HentResultater(ArrangementRequest request)
        {
            return _context.Arrangement
                .Where(a=> a.Id == request.ArrangementId)
                .SelectMany(a => a.Saker)
                .SelectMany(s => s.Voteringer)
                .Where(v => v.Publisert)
                .ProjectTo<VoteringResultatDto>(_mapper.ConfigurationProvider)
                .AsAsyncEnumerable();
        }

        public async Task<ArrangementInfo?> HentArrangementInfoAsync(ArrangementRequest request, CancellationToken cancellationToken = default)
        {
            return await _cache.GetOrAddAsync($"ArrangementInfo({request.ArrangementId})", async () =>
            {
                return await _context.Arrangement
                    .Where(a=> a.Id == request.ArrangementId)
                    .ProjectTo<ArrangementInfo>(_mapper.ConfigurationProvider)
                    .SingleOrDefaultAsync(cancellationToken: cancellationToken);
            },  DateTimeOffset.Now.AddSeconds(60));
        }
    }
}
