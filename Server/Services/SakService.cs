using AutoMapper;
using AutoMapper.QueryableExtensions;
using LazyCache;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Stemmesystem.Data;
using Stemmesystem.Data.Entities;
using Stemmesystem.Core;
using Stemmesystem.Core.Interfaces;
using Stemmesystem.Core.Models;

namespace Stemmesystem.Server.Services;

[Authorize]
public class SakService : ISakService
{
    private readonly StemmesystemContext _context;
    private readonly IMapper _mapper;
    private readonly NotificationManager _notificationManager;
    private readonly IAppCache _cache;

    public SakService(IMapper mapper, NotificationManager notificationManager, StemmesystemContext context, IAppCache cache)
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

    public async Task<bool> ErNummerBrukt(int arrangementId, string? nummer, CancellationToken cancellationToken = default)
    {
        return await _context.Arrangement
            .Where(a => a.Id == arrangementId)
            .SelectMany(a => a.Saker)
            .AnyAsync(s => s.Nummer == nummer, cancellationToken: cancellationToken);
    }

    [Authorize(Roles = "admin")]
    public async Task<LagreResult<SakDto>> LagreNySak(SakInputModel input, CancellationToken cancellationToken = default)
    {
        var errors = new Dictionary<string, List<string>>();
        var arrangement = await _context.Arrangement
            .Where(a => a.Id == input.ArrangementId)
            .Include(a => a.Saker)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        if (arrangement == null)
            throw new StemmeException($"Arrangement med id {input.ArrangementId} ble ikke funnet");
            
        if ( await ErNummerBrukt(input.ArrangementId, input.Nummer, cancellationToken))
        {
            errors.Add(nameof(input.Nummer), ["Saknummer er allerede brukt"]);
            return new LagreResult<SakDto>(null, errors);
        }
        
        var sak = _mapper.Map<Sak>(input);
        arrangement.LeggTil(sak);
        await _context.SaveChangesAsync(cancellationToken);
        var dto = _mapper.Map<SakDto>(sak);
        return new LagreResult<SakDto>(dto, errors);
    }

    [Authorize(Roles = "admin")]
    public async Task<SakDto> OppdaterSak(SakInputModel input, CancellationToken cancellationToken = default)
    {
        var sak = await _context.Arrangement
            .Where(a => a.Id == input.ArrangementId)
            .SelectMany(a => a.Saker)
            .Where(s => s.Id == input.Id)
            .FirstAsync(cancellationToken: cancellationToken);

        sak.Nummer = input.Nummer!;
        sak.Tittel = input.Tittel!;
        sak.Beskrivelse = input.Beskrivelse;

        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<SakDto>(sak);
    }
    [Authorize(Roles = "admin")]
    public async Task<LagreResult<VoteringDto>> LagreNyVotering(VoteringInputModel input, CancellationToken cancellationToken = default)
    {
        var errors = new Dictionary<string, List<string>>();
        var sak = await _context.Sak
            .Where(a => a.Id == input.SakId)
            .Include(a => a.Voteringer)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        if (sak == null)
            throw new StemmeException($"Sak med id {input.SakId} ble ikke funnet");

        var votering = _mapper.Map<Votering>(input);
        sak.LeggTil(votering);
        await _context.SaveChangesAsync(cancellationToken);
        var nyVotering = _mapper.Map<AdminVoteringDto>(votering);
        await _notificationManager.ForAdmin(sak.ArrangementId).NyVotering(new NyVoteringEvent(nyVotering), cancellationToken);
        return new LagreResult<VoteringDto>(nyVotering, errors);
    }

    [Authorize(Roles = "admin")]
    public async Task<LagreResult<VoteringDto>> OppdaterVotering(VoteringInputModel input, CancellationToken cancellationToken = default)
    {
        var votering = await _context.Votering
            .Where(a => a.Id == input.Id)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        if (votering == null)
            return LagreResult.Error<VoteringDto>("Fant ikke votering å oppdatere");
        _mapper.Map(input, votering);
        await _context.SaveChangesAsync(cancellationToken);
        return LagreResult.Success(_mapper.Map<VoteringDto>(votering));
    }


        
    public async Task<ICollection<AdminSakDto>> HentSaker(SakerRequest request, CancellationToken cancellationToken = default)
    {
        return await _context.Arrangement
            .Where(a => a.Id == request.ArrangementId)
            .AsSplitQuery()
            .SelectMany(a => a.Saker)
            .ProjectTo<AdminSakDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken: cancellationToken);
    }

    public Task<HentResult<SakInfoDto>> HentSakInfo(SakRequest request, CancellationToken cancellationToken = default)
    {
        return _cache.GetOrAddAsync(request.ToString(), async () =>
        {
            var sak = await _context.Arrangement
                .SelectMany(a => a.Saker)
                .Where(s => s.Id == request.SakId)
                .ProjectTo<SakInfoDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);
            return new HentResult<SakInfoDto>(sak);
        }, DateTimeOffset.Now.AddMinutes(1));
    }
}