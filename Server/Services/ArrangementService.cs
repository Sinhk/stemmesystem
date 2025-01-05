using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Stemmesystem.Data;
using Stemmesystem.Server.Data.Entities;
using Stemmesystem.Shared;
using Stemmesystem.Shared.Interfaces;
using Stemmesystem.Shared.Models;
using ZiggyCreatures.Caching.Fusion;

namespace Stemmesystem.Server.Services;

[Authorize]
public class ArrangementService : IArrangementService
{
    private readonly StemmesystemContext _context;
    private readonly IFusionCache _cache;

    public ArrangementService(StemmesystemContext context, IFusionCache cache)
    {
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
            .ToDtos()
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ArrangementDto> NyttArrangement(ArrangementInputModel model)
    {
        ArgumentNullException.ThrowIfNull(model.Navn, nameof(model.Navn));
        if (!await IsNameAvailable(model.Navn!))
            throw new StemmeException($"Det finnes allered et arrangement med navn {model.Navn}");

        var entity = new Arrangement(model.Navn)
        {
            Beskrivelse = model.Beskrivelse,
            Startdato = model.Startdato,
            Sluttdato = model.Sluttdato
        };

        _context.Arrangement.Add(entity);
        await _context.SaveChangesAsync();
        return entity.ToDto();
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
            .ToInfos()
            .ToListAsync(cancellationToken);
    }

    public async Task<ArrangementDto?> HentArrangementAsync(int id, CancellationToken cancellationToken = default)
    {
        var arrangement = await GetSingleQuery(_context)
            .Where(a => a.Id == id)
            .ToDtos()
            .FirstOrDefaultAsync(cancellationToken);
        return arrangement;
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

    public async Task<IReadOnlyCollection<VoteringDto>> FinnAktiveVoteringer(ArrangementRequest request, CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrSetAsync<IReadOnlyCollection<VoteringDto>>($"aktive-{request.ArrangementId}", async token =>
        {
            return await _context.Arrangement
                .AsSplitQuery()
                .Where(a => a.Id == request.ArrangementId)
                .SelectMany(a => a.Saker)
                .SelectMany(s => s.Voteringer)
                .Where(v => v.Aktiv)
                .ToDtos()
                .ToArrayAsync(cancellationToken: token);

        }, options =>
        {
            options.Duration = TimeSpan.FromSeconds(2);
            options.SetFailSafe(true, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(10));
        }, token: cancellationToken);
    }

    public async Task<IReadOnlyCollection<VoteringResultatDto>> HentResultater(ArrangementRequest request, CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrSetAsync($"resultater-{request.ArrangementId}", async token =>
        {
            return await _context.Arrangement
                .Where(a=> a.Id == request.ArrangementId)
                .SelectMany(a => a.Saker)
                .SelectMany(s => s.Voteringer)
                .Where(v => v.Publisert)
                .Select(v => new VoteringResultatDto
                {
                    Id = v.Id,
                    Beskrivelse = v.Beskrivelse,
                    Tittel = v.Tittel,
                    SakNavn = v.Sak.Tittel,
                    AvgitteStemmer = v.AvgitStemme.Count,
                    DelegaterTilstede = v.DelegaterTilstede,
                    SakNummer = v.Sak.Nummer,
                    Valg = v.Valg.Select(valg => valg.ToDto()).ToList(),
                })
                .ToArrayAsync(cancellationToken: token);
        }, token: cancellationToken);
    }

    public async Task<ArrangementInfo?> HentArrangementInfoAsync(ArrangementRequest request)
    {
        return await _cache.GetOrSetAsync<ArrangementInfo?>($"ArrangementInfo({request.ArrangementId})", async token =>
        {
            return await _context.Arrangement
                .Where(a=> a.Id == request.ArrangementId)
                .ToInfos()
                .SingleOrDefaultAsync(token);
        });
    }

    public async Task<TilstedeCountResponse> GetTilstedeCount(TilstedeCountRequest request, CancellationToken cancellationToken = default)
    {
        var count = await _context.Arrangement.Where(a => a.Id == request.ArrangementId)
            .Select(a => a.Delegater.Count(d => d.TilStede))
            .SingleOrDefaultAsync(cancellationToken);

        return new TilstedeCountResponse(count);
    }
}