using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Stemmesystem.Data;
using Stemmesystem.Server.Data.Entities;
using Stemmesystem.Shared;
using Stemmesystem.Shared.Interfaces;
using Stemmesystem.Shared.Models;
using ZiggyCreatures.Caching.Fusion;

namespace Stemmesystem.Server.Services
{
    [Authorize]
    public class ArrangementService : IArrangementService
    {
        private readonly StemmesystemContext _context;
        private readonly IMapper _mapper;
        private readonly IFusionCache _cache;

        public ArrangementService(StemmesystemContext context, IMapper mapper, IFusionCache cache)
        {
            _mapper = mapper;
            _cache = cache;
            _context = context;
        }

        public async Task<ArrangementDto?> GetArrangementAsync(GetArrangementRequest request, CancellationToken cancellationToken = default)
        {
            if (request.Id != null)
                return await GetArrangementAsync(request.Id.Value, cancellationToken);
            if (request.Name != null)
                return await GetArrangementByNameAsync(request.Name, cancellationToken);
            throw new VotingException("request må ha Id eller navn");
        }

        public async Task<ArrangementDto?> GetArrangementByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return await GetSingleQuery(_context)
                .Where(a => a.Name == name)
                .ProjectTo<ArrangementDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<ArrangementDto> CreateArrangement(ArrangementInputModel model)
        {
            if (!await IsNameAvailable(model.Name!))
                throw new VotingException($"Det finnes allered et arrangement med navn {model.Name}");
            var entity = _mapper.Map<Arrangement>(model);
            _context.Arrangements.Add(entity);
            await _context.SaveChangesAsync();
            return _mapper.Map<ArrangementDto>(entity);
        }

        public async Task<bool> IsNameAvailable(string name)
        {
            return await _context.Arrangements.AllAsync(a => a.Name != name);
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

        public async Task<List<ArrangementInfo>> GetArrangementsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Arrangements
                .AsSplitQuery()
                .Include(a=> a.Delegates)
                .Include(a=> a.Cases)
                .ThenInclude(s => s.Ballots)
                .Where(a=> a.Active == true)
                .ProjectTo<ArrangementInfo>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }

        public async Task<ArrangementDto?> GetArrangementAsync(int id, CancellationToken cancellationToken = default)
        {
            var arrangement = await GetSingleQuery(_context)
                .Where(a => a.Id == id)
                .ProjectTo<ArrangementDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);
            return arrangement;
        }

        public async Task<ArrangementDto> UpdateArrangement(ArrangementInputModel model)
        {
            var arrangement = await _context.Arrangements
                .Where(a => a.Id == model.Id)
                .FirstOrDefaultAsync();
            if (arrangement == null)
                throw new VotingException($"Fant ingen arrangement med id {model.Id} å oppdatere");

            arrangement.Description = model.Description;
            arrangement.StartDate = model.StartDate;
            arrangement.EndDate = model.EndDate;
            
            await _context.SaveChangesAsync();
            return (await GetArrangementAsync(arrangement.Id))!;
        }

        public async Task<IReadOnlyCollection<BallotDto>> GetActiveBallots(ArrangementRequest request, CancellationToken cancellationToken = default)
        {
            return await _cache.GetOrSetAsync<IReadOnlyCollection<BallotDto>>($"aktive-{request.ArrangementId}", async token =>
            {
                return await _context.Arrangements
                    .AsSplitQuery()
                    .Where(a => a.Id == request.ArrangementId)
                    .SelectMany(a => a.Cases)
                    .SelectMany(s => s.Ballots)
                    .Where(v => v.Active)
                    .ProjectTo<BallotDto>(_mapper.ConfigurationProvider)
                    .ToArrayAsync(cancellationToken: token);

            }, options =>
            {
                options.Duration = TimeSpan.FromSeconds(2);
                options.SetFailSafe(true, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(10));
            }, token: cancellationToken);
        }

        public async Task<IReadOnlyCollection<BallotResultDto>> GetResults(ArrangementRequest request, CancellationToken cancellationToken = default)
        {
            return await _cache.GetOrSetAsync($"resultater-{request.ArrangementId}", async token =>
            {
                return await _context.Arrangements
                    .Where(a=> a.Id == request.ArrangementId)
                    .SelectMany(a => a.Cases)
                    .SelectMany(s => s.Ballots)
                    .Where(v => v.Published)
                    .ProjectTo<BallotResultDto>(_mapper.ConfigurationProvider)
                    .ToArrayAsync(cancellationToken: token);
            }, token: cancellationToken);
        }

        public async Task<ArrangementInfo?> GetArrangementInfoAsync(ArrangementRequest request)
        {
            return await _cache.GetOrSetAsync<ArrangementInfo?>($"ArrangementInfo({request.ArrangementId})", async token =>
            {
                return await _context.Arrangements
                    .Where(a=> a.Id == request.ArrangementId)
                    .ProjectTo<ArrangementInfo>(_mapper.ConfigurationProvider)
                    .SingleOrDefaultAsync(token);
            });
        }

        public async Task<PresentCountResponse> GetPresentCount(PresentCountRequest request, CancellationToken cancellationToken = default)
        {
            var count = await _context.Arrangements.Where(a => a.Id == request.ArrangementId)
                .Select(a => a.Delegates.Count(d => d.Present))
                .SingleOrDefaultAsync(cancellationToken);

            return new PresentCountResponse(count);
        }
    }
}
