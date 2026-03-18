using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Stemmesystem.Data;
using Stemmesystem.Data.Entities;
using Stemmesystem.Shared;
using Stemmesystem.Shared.Interfaces;
using Stemmesystem.Shared.Models;
using ZiggyCreatures.Caching.Fusion;

namespace Stemmesystem.Server.Services;

[Authorize]
public class SakService : ISakService
{
    private readonly StemmesystemContext _context;
    private readonly NotificationManager _notificationManager;
    private readonly IFusionCache _cache;

    public SakService(NotificationManager notificationManager, StemmesystemContext context, IFusionCache cache)
    {
        _notificationManager = notificationManager;
        _context = context;
        _cache = cache;
    }

    public async Task<SakDto?> HentSak(SakRequest request, CancellationToken cancellationToken = default)
    {
        var sak = await _context.Sak
            .Include(s => s.Voteringer)
            .Where(s => s.Id == request.SakId)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        return sak?.ToDto();
    }

    public async Task<VoteringDto> HentVotering(HentVoteringRequest request, CancellationToken cancellationToken = default)
    {
        var (sakId, voteringId) = request;
        var votering = await _context.Votering
            .Where(v => v.SakId == sakId && v.Id == voteringId)
            .FirstAsync(cancellationToken: cancellationToken);
        return votering.ToDto();
    }

    public async Task<ICollection<VoteringDto>> HentVoteringer(HentVoteringerRequest request, CancellationToken cancellationToken = default)
    {
        var voteringer = await _context.Votering
            .Where(v => v.SakId == request.SakId)
            .ToListAsync(cancellationToken);
        return voteringer.Select(v => v.ToDto()).ToList();
    }

    public async Task<bool> ErNummerBrukt(int arrangementId, string? nummer)
    {
        return await _context.Arrangement
            .Where(a => a.Id == arrangementId)
            .SelectMany(a => a.Saker)
            .AnyAsync(s => s.Nummer == nummer);
    }

    [Authorize(Roles = "admin")]
    public async Task<LagreResult<SakDto>> LagreNySak(SakInputModel model)
    {
        var errors = new Dictionary<string, List<string>>();
        var arrangement = await _context.Arrangement
            .Where(a => a.Id == model.ArrangementId)
            .Include(a => a.Saker)
            .FirstOrDefaultAsync();
        if (arrangement == null)
            throw new StemmeException($"Arrangement med id {model.ArrangementId} ble ikke funnet");
            
        if (await ErNummerBrukt(model.ArrangementId, model.Nummer))
        {
            errors.Add(nameof(model.Nummer), new List<string>{"Saknummer er allerede brukt"});
            return new LagreResult<SakDto>(null, errors);
        }
        
        var sak = new Sak(model.Nummer!, model.Tittel!) { Beskrivelse = model.Beskrivelse };
        foreach (var voteringModel in model.Voteringer)
            sak.LeggTil(Votering.FraInputModel(voteringModel));
        arrangement.LeggTil(sak);
        await _context.SaveChangesAsync();
        var dto = sak.ToDto();
        return new LagreResult<SakDto>(dto, errors);
    }

    [Authorize(Roles = "admin")]
    public async Task<SakDto> OppdaterSak(SakInputModel model)
    {
        var sak = await _context.Arrangement
            .Where(a => a.Id == model.ArrangementId)
            .SelectMany(a => a.Saker)
            .Where(s => s.Id == model.Id)
            .FirstAsync();

        sak.Nummer = model.Nummer!;
        sak.Tittel = model.Tittel!;
        sak.Beskrivelse = model.Beskrivelse;

        await _context.SaveChangesAsync();
        return sak.ToDto();
    }

    [Authorize(Roles = "admin")]
    public async Task<LagreResult<VoteringDto>> LagreNyVotering(VoteringInputModel model)
    {
        var errors = new Dictionary<string, List<string>>();
        var sak = await _context.Sak
            .Where(a => a.Id == model.SakId)
            .Include(a => a.Voteringer)
            .FirstOrDefaultAsync();
        if (sak == null)
            throw new StemmeException($"Sak med id {model.SakId} ble ikke funnet");

        var votering = Votering.FraInputModel(model);
        sak.LeggTil(votering);
        await _context.SaveChangesAsync();
        var nyVotering = votering.ToAdminDto();
        await _notificationManager.ForAdmin(sak.ArrangementId).NyVotering(new NyVoteringEvent(nyVotering));
        return new LagreResult<VoteringDto>(nyVotering, errors);
    }

    [Authorize(Roles = "admin")]
    public async Task<LagreResult<VoteringDto>> OppdaterVotering(VoteringInputModel model)
    {
        var votering = await _context.Votering
            .Where(a => a.Id == model.Id)
            .FirstOrDefaultAsync();
        if (votering == null)
            return LagreResult<VoteringDto>.Error("Fant ikke votering å oppdatere");
        votering.OppdaterFraInputModel(model);
        await _context.SaveChangesAsync();
        return LagreResult.Success(votering.ToDto());
    }

    public async Task<ICollection<AdminSakDto>> HentSaker(SakerRequest request)
    {
        var saker = await _context.Sak
            .Where(s => s.ArrangementId == request.ArrangementId)
            .AsSplitQuery()
            .Include(s => s.Voteringer)
                .ThenInclude(v => v.AvgitStemme)
            .Include(s => s.Voteringer)
                .ThenInclude(v => v.Stemmer)
            .ToListAsync();
        return saker.Select(s => s.ToAdminDto()).ToList();
    }

    public async Task<HentResult<SakInfoDto>> HentSakInfo(SakRequest request, CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrSetAsync<HentResult<SakInfoDto>>($"sakinfo-{request.SakId}", async token =>
        {
            var sak = await _context.Sak
                .Where(s => s.Id == request.SakId)
                .FirstOrDefaultAsync(token);
            return new HentResult<SakInfoDto>(sak?.ToInfoDto());
        }, token: cancellationToken);
    }
}
