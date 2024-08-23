using System.Globalization;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ProtoBuf.Grpc;
using Stemmesystem.Data;
using Stemmesystem.Data.Entities;
using Stemmesystem.Core;
using Stemmesystem.Core.Interfaces;
using Stemmesystem.Core.Models;
using Stemmesystem.Core.Tools;
using Stemmesystem.Server.Auth;

namespace Stemmesystem.Server.Services;

[Authorize]
public class DelegatService(IKeyGenerator keyGenerator, StemmesystemContext context, IMapper mapper) : IDelegatService, IAdminDelegatService
{
    private readonly StemmesystemContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IKeyGenerator _keyGenerator = keyGenerator;

    public async Task<HentDelegatResult> HentDelegatInfo(CallContext context = default)
    {
        var cancellationToken = context.CancellationToken;
        var user = context.ServerCallContext?.GetHttpContext().User;
        if (user is null || !user.IsInRole("Delegat"))
            return new HentDelegatResult(null);
        
        return new HentDelegatResult(await ValiderKode(user.GetDelegatkode(), cancellationToken));
    }
    
    public async Task<DelegatDto?> HentDelegat(int arrangementId, int delegatId)
    {
        return await _context.Delegat
            .Where(d => d.ArrangementId == arrangementId && d.Id == delegatId)
            .ProjectTo<DelegatDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();
    }

    [Authorize(Roles = "admin")]
    async Task<AdminDelegatDto?> IAdminDelegatService.HentDelegat(HentDelegatRequest request, CancellationToken cancellationToken)
    {
        var (arrangementId, delgatId) = request;
        if (delgatId == null)
            throw new StemmeException("DelegatId er påkrevd");
        return await _context.Delegat
            .Where(d => d.ArrangementId == arrangementId && d.Id == delgatId)
            .ProjectTo<AdminDelegatDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
    }
    [Authorize(Roles = "admin")]
    public async Task<AdminDelegatDto> OppdaterDelegat(DelegatInputModel model,
        CancellationToken cancellationToken = default)
    {
        var delegat = await _context.Delegat
            .Where(d => d.ArrangementId == model.ArrangementId)
            .Where(d => d.Id == model.Id)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        if (delegat == null)
            throw new StemmeException($"Fant ingen delegat med id {model.Id} å oppdatere");

        if(model.Delegatnummer.HasValue)
            delegat.Delegatnummer = model.Delegatnummer.Value;
        delegat.Navn = model.Navn;
        delegat.Gruppe = model.Gruppe;
        delegat.Epost = model.Epost;
        delegat.Telefon = model.Telefon;
            
        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<AdminDelegatDto>(delegat);
    }

    [Authorize(Roles = "admin")]
    public async Task<ICollection<DelegatDto>> HentDelegater(int arrangementId)
    {
        return await _context.Delegat
            .Where(d => d.ArrangementId == arrangementId)
            .ProjectTo<DelegatDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    async Task<ICollection<AdminDelegatDto>> IAdminDelegatService.HentDelegater(HentDelegatRequest request, CancellationToken cancellationToken)
    {
        return await _context.Delegat
            .Where(d => d.ArrangementId == request.ArrangementId)
            .ProjectTo<AdminDelegatDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken: cancellationToken);

    }

    public async Task<bool> IsValidNo(int arrangement, int number)
    {
        return await _context.Delegat
            .Where(d => d.ArrangementId == arrangement)
            .AllAsync(d => d.Delegatnummer != number);
    }
    
    [Authorize(Roles = "admin")]
    public async Task<AdminDelegatDto> RegistrerNyDelegat(DelegatInputModel model,
        CancellationToken cancellationToken = default)
    {
        if (!await IsValidNo(model.ArrangementId, model.Delegatnummer!.Value))
            throw new StemmeException("Delegatnummer er allerede brukt");

        var delegat = _mapper.Map<Delegat>(model);
        delegat.ArrangementId = model.ArrangementId;
        delegat.Delegatkode = _keyGenerator.GenerateKey(4);
        _context.Delegat.Add(delegat);
        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<AdminDelegatDto>(delegat);
    }

    public async Task<DelegatDto?> ValiderKode(string delegatKode, CancellationToken cancellationToken = default)
    {
        delegatKode = delegatKode.ToUpper(CultureInfo.InvariantCulture);
        var delegat = await _context.Delegat
            .Include(d => d.Arrangement)
            .Where(d => d.Delegatkode == delegatKode)
            .ProjectTo<DelegatDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
        return delegat;
    }
}