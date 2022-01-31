using AutoMapper;
using Duende.IdentityServer.Extensions;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ProtoBuf.Grpc;
using StemmeSystem.Data;
using Stemmesystem.Server.Data.Entities;
using Stemmesystem.Server.Data.Repositories;
using Stemmesystem.Shared;
using Stemmesystem.Shared.Interfaces;
using Stemmesystem.Shared.Models;
using Stemmesystem.Shared.Tools;

namespace Stemmesystem.Server.Services;

[Authorize]
public class StemmeService : IStemmeService, IAdminStemmeService
{
    private readonly IDelegatRepository _delegatRepository;
    private readonly StemmesystemContext _context;
    private readonly IKeyHasher _keyHasher;
    private readonly IArrangementRepository _arrangementRepository;
    private readonly NotificationManager _notificationManager;
    private readonly IMapper _mapper;

    public StemmeService(IDelegatRepository delegatRepository, IArrangementRepository arrangementRepository ,StemmesystemContext context, IKeyHasher keyHasher, NotificationManager notificationManager, IMapper mapper)
    {
        _delegatRepository = delegatRepository;
        _arrangementRepository = arrangementRepository;
        _context = context;

        _keyHasher = keyHasher;
        _notificationManager = notificationManager;
        _mapper = mapper;
    }

    public async Task<List<StemmeDto>> AvgiStemmeAsync(AvgiStemmeRequest request, CallContext context = default)
    {
        var cancellationToken = context.CancellationToken;
        var delegatkode =  context.ServerCallContext?.GetHttpContext().User.GetSubjectId();
        if (delegatkode == null)
            throw new StemmeException($"Fant ikke delegatkode");

        var delegat = await _delegatRepository.ValiderKode(delegatkode, cancellationToken);
        if (delegat == null)
            throw new StemmeException($"Ugyldig delegat {delegatkode}");

        _context.Attach(delegat);
        var votering = await _context.Votering
            .AsSingleQuery()
            .Include(v=> v.Stemmer)
            .Include(v => v.AvgitStemme)
            .Include(v=> v.Sak)
            .SingleOrDefaultAsync(v => v.Id == request.VoteringId, cancellationToken);

        if (votering == null)
        {
            throw new StemmeException($"Ugyldig votering {request.VoteringId}");
        }
            
        if (votering.Aktiv == false)
        {
            throw new StemmeException("Votering er ferdig eller har ikke startet enda");
        }

        var stemmer = votering.RegistrerStemme(request.ValgIder, delegat, delegatkode, _keyHasher, _notificationManager);
            
        await _context.SaveChangesAsync(cancellationToken);
        var dto = _mapper.Map<List<StemmeDto>>(stemmer);
        return dto;
    }

    [Authorize(Roles = "admin")]
    public async Task StartVotering(AdminStemmeRequest request, CancellationToken cancellationToken = default)
    {
        var arrangement = await _arrangementRepository.HentArrangementAsync(request.ArrangementId, cancellationToken);
        if(arrangement == null)
            return;
        _context.Attach(arrangement);
        var votering = arrangement.FinnVotering(request.VoteringId);
        if (votering == null)
            throw new StemmeException("Fant ikke valgt votering");
        votering.StartVotering();

        await _context.SaveChangesAsync(cancellationToken);
            
        _notificationManager.ForArrangement(request.ArrangementId).OnVoteringStartet(new VoteringStartetEvent(votering.Id, votering.SakId));
    }

    [Authorize(Roles = "admin")]
    public async Task StoppVotering(AdminStemmeRequest request, CancellationToken cancellationToken = default)
    {
        var arrangement = await _arrangementRepository.HentArrangementAsync(request.ArrangementId, cancellationToken);
        if(arrangement == null)
            return;
        _context.Attach(arrangement);
        var votering = arrangement.FinnVotering(request.VoteringId);
        if (votering == null)
            throw new StemmeException("Fant ikke valgt votering");
        votering.AvsluttVotering();
        await _context.SaveChangesAsync(cancellationToken);
            
        _notificationManager.ForArrangement(request.ArrangementId).OnVoteringStoppet(new(votering.Id));
    }

        
    public async Task<HarStemmtResult> HarStemmt(StemmeRequest request, CallContext context = default)
    {
        var cancellationToken = context.CancellationToken;
        var delegat = await FinnDelegat(context, cancellationToken);
        var harStemmt = await _context.Votering
            .Where(v => v.Id == request.VoteringId)
            .SelectMany(v => v.AvgitStemme)
            .AnyAsync(s => s.Id == delegat.Id, cancellationToken);

        List<StemmeDto>? stemmer = null;
        if (harStemmt)
        {
            var tmp = await GetFinnStemmeQuery(_context, request.VoteringId, delegat, cancellationToken);
            stemmer = _mapper.Map<List<StemmeDto>>(tmp);
        }
        var result = new HarStemmtResult(harStemmt, stemmer);
        return result;
    }

    private async Task<Delegat> FinnDelegat(CallContext context, CancellationToken cancellationToken)
    {
        var delegatkode = context.ServerCallContext?.GetHttpContext().User.GetSubjectId();
        if (delegatkode == null)
            throw new StemmeException($"Fant ikke delegatkode");
            
        var delegat = await _delegatRepository.ValiderKode(delegatkode, cancellationToken);
        if (delegat == null)
            throw new StemmeException($"Ugyldig delegat {delegatkode}");
        return delegat;
    }

    public async Task<StemmeDto> FinnStemme(int voteringId, Guid stemmeId, CancellationToken cancellationToken = default)
    {
        var stemme = await _context.Votering
            .Where(v => v.Id == voteringId)
            .SelectMany(v=> v.Stemmer)
            .Where(s => s.Id == stemmeId)
            .FirstOrDefaultAsync(cancellationToken);
            
        return _mapper.Map<StemmeDto>(stemme);
    }

    public async Task<List<StemmeDto>> FinnStemmer(int voteringId, string delegatKode, CancellationToken cancellationToken = default)
    {
        var stemmer = await GetFinnStemmeQuery(_context, voteringId, delegatKode, cancellationToken);
        return _mapper.Map<List<StemmeDto>>(stemmer);
    }

    private async Task<List<Stemme>> GetFinnStemmeQuery(StemmesystemContext context, int voteringId, string delegatkode, CancellationToken cancellationToken = default)
    {
        var delegat = await _delegatRepository.ValiderKode(delegatkode, cancellationToken);
        if (delegat == null)
            throw new StemmeException($"Ugyldig delegat {delegatkode}");
        return await GetFinnStemmeQuery(context, voteringId, delegat, cancellationToken);
    }
        
    private async Task<List<Stemme>> GetFinnStemmeQuery(StemmesystemContext context, int voteringId, Delegat delegat, CancellationToken cancellationToken = default)
    {
        var votering = await context.Votering
            .Where(v => v.Id == voteringId)
            .Include(v=> v.Stemmer)
            .FirstOrDefaultAsync(cancellationToken);
        if (votering == null)
            throw new StemmeException($"Votering med id {voteringId} ble ikke funnet");

        var stemmer = votering.Hemmelig
            ? votering.Stemmer.Where(s => _keyHasher.VerifyHash(s.StemmeHash, delegat.Delegatkode)).ToList()
            : votering.Stemmer.Where(s => s.DelegatId == delegat.Id).ToList();

        return stemmer;
    }

    [Authorize(Roles = "admin")]
    public async Task LukkVotering(AdminStemmeRequest request, CancellationToken cancellationToken = default)
    {
        var (arrangementId, voteringId) = request;
        var arrangement = await _arrangementRepository.HentArrangementAsync(arrangementId, cancellationToken);
        if(arrangement == null)
            return;
        _context.Attach(arrangement);
        var votering = arrangement.FinnVotering(voteringId);
        if (votering == null)
            throw new StemmeException("Fant ikke valgt votering");
        if (votering.Aktiv)
        {
            votering.AvsluttVotering();
            _notificationManager.ForArrangement(arrangementId).OnVoteringStoppet(new VoteringStoppetEvent(votering.Id));
        }
        votering.LukkVotering();
        await _context.SaveChangesAsync(cancellationToken);
            
        _notificationManager.ForArrangement(arrangementId).OnVoteringLukket(new VoteringLukketEvent(votering.Id));
    }
        
    [Authorize(Roles = "admin")]
    public async Task PubliserVotering(AdminStemmeRequest request, CancellationToken cancellationToken = default)
    {
        var (arrangementId, voteringId) = request;
        var arrangement = await _arrangementRepository.HentArrangementAsync(arrangementId, cancellationToken);
        if(arrangement == null)
            return;
        _context.Attach(arrangement);
        var votering = arrangement.FinnVotering(voteringId);
        if (votering == null)
            throw new StemmeException("Fant ikke valgt votering");
        votering.PubliserVotering();
        await _context.SaveChangesAsync(cancellationToken);
            
        _notificationManager.ForArrangement(arrangementId).OnVoteringPublisert(new VoteringPublisertEvent(votering.Id, votering.SakId));
    }

    [Authorize(Roles = "admin")]
    public async Task<VoteringInputModel> KopierVotering(AdminStemmeRequest request, CancellationToken cancellationToken = default)
    {
        var (arrangementId, voteringId) = request;
        var arrangement = await _arrangementRepository.HentArrangementAsync(arrangementId, cancellationToken);
        if(arrangement == null)
            throw new StemmeException("Fant ikke valgt arrangement");
        _context.Attach(arrangement);
        var votering = arrangement.FinnVotering(voteringId);
        if (votering == null)
            throw new StemmeException("Fant ikke valgt votering");
            
        var kopi = votering.Kopier();
        return _mapper.Map<VoteringInputModel>(kopi);
    }

    [Authorize(Roles = "admin")]
    public async Task ResetVotering(AdminStemmeRequest request, CancellationToken cancellationToken = default)
    {
        var (arrangementId, voteringId) = request;
        var arrangement = await _arrangementRepository.HentArrangementAsync(arrangementId, cancellationToken);
        if(arrangement == null)
            return;
        _context.Attach(arrangement);
        var votering = arrangement.FinnVotering(voteringId);
        if(votering == null) return;
        votering.StartTid = null;
        votering.SluttTid = null;
        votering.Lukket = false;
        votering.Publisert = false;
        await _context.SaveChangesAsync(cancellationToken);
    }
}