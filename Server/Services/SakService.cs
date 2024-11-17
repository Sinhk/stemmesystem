using AutoMapper;
using AutoMapper.QueryableExtensions;
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
    private readonly IMapper _mapper;
    private readonly NotificationManager _notificationManager;
    private readonly IFusionCache _cache;

    public SakService(IMapper mapper, NotificationManager notificationManager, StemmesystemContext context, IFusionCache cache)
    {
        _mapper = mapper;
        _notificationManager = notificationManager;
        _context = context;
        _cache = cache;
    }
    public async Task<SakDto?> HentSak(SakRequest request, CancellationToken cancellationToken = default)
    {
        var sak = await _context.Arrangement
            .SelectMany(a => a.Saker)
            .Where(s => s.Id == request.SakId)
            .ProjectTo<SakDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        return sak;
    }

    public async Task<VoteringDto> HentVotering(HentVoteringRequest request, CancellationToken cancellationToken = default)
    {
        var (sakId, voteringId) = request;
        return await _context.Arrangement
            .SelectMany(a => a.Saker)
            .Where(s => s.Id == sakId)
            .SelectMany(s=> s.Voteringer)
            .Where(v=> v.Id == voteringId)
            .ProjectTo<VoteringDto>(_mapper.ConfigurationProvider)
            .FirstAsync(cancellationToken: cancellationToken);
    }

    public async Task<ICollection<VoteringDto>> HentVoteringer(HentVoteringerRequest request, CancellationToken cancellationToken = default)
    {
        return await _context.Arrangement
            .AsSplitQuery()
            .SelectMany(a => a.Saker)
            .Where(s => s.Id == request.SakId)
            .SelectMany(s=> s.Voteringer)
            .ProjectTo<VoteringDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
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
            
        if ( await ErNummerBrukt(model.ArrangementId, model.Nummer))
        {
            errors.Add(nameof(model.Nummer), new List<string>{"Saknummer er allerede brukt"});
            return new LagreResult<SakDto>(null, errors);
        }
        
        var sak = _mapper.Map<Sak>(model);
        arrangement.LeggTil(sak);
        await _context.SaveChangesAsync();
        var dto = _mapper.Map<SakDto>(sak);
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
        return _mapper.Map<SakDto>(sak);
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

        var votering = _mapper.Map<Votering>(model);
        sak.LeggTil(votering);
        await _context.SaveChangesAsync();
        var nyVotering = _mapper.Map<AdminVoteringDto>(votering);
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
        _mapper.Map(model, votering);
        await _context.SaveChangesAsync();
        return LagreResult.Success(_mapper.Map<VoteringDto>(votering));
    }


        
    public async Task<ICollection<AdminSakDto>> HentSaker(SakerRequest request)
    {
        return await _context.Arrangement
            .Where(a => a.Id == request.ArrangementId)
            .AsSplitQuery()
            .SelectMany(a => a.Saker)
            .ProjectTo<AdminSakDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<HentResult<SakInfoDto>> HentSakInfo(SakRequest request, CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrSetAsync<HentResult<SakInfoDto>>($"sakinfo-{request.SakId}", async token =>
        {
            var sak = await _context.Arrangement
                .SelectMany(a => a.Saker)
                .Where(s => s.Id == request.SakId)
                .ProjectTo<SakInfoDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(token);
            return new HentResult<SakInfoDto>(sak);
        }, token: cancellationToken);
    }
}