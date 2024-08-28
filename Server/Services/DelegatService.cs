using AutoMapper;
using AutoMapper.QueryableExtensions;
using Duende.IdentityServer.Extensions;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ProtoBuf.Grpc;
using Stemmesystem.Data;
using Stemmesystem.Data.Entities;
using Stemmesystem.Shared;
using Stemmesystem.Shared.Interfaces;
using Stemmesystem.Shared.Models;
using Stemmesystem.Shared.Tools;

namespace Stemmesystem.Server.Services;

[Authorize]
public class DelegatService : IDelegatService, IAdminDelegatService
{
    private readonly StemmesystemContext _context;
    private readonly IMapper _mapper;
    private readonly IKeyGenerator _keyGenerator;

    public DelegatService(IKeyGenerator keyGenerator, StemmesystemContext context, IMapper mapper)
    {
        _keyGenerator = keyGenerator;
        _context = context;
        _mapper = mapper;
    }

    public async Task<HentDelegatResult> HentDelegatInfo(CallContext context = default)
    {
        var user = context.ServerCallContext?.GetHttpContext().User;
        if (user == null || !user.IsInRole("Delegat"))
            return new HentDelegatResult(null);
        
        return new HentDelegatResult(await ValiderKode(user.GetSubjectId()));
    }
    
    public async Task<DelegatDto?> HentDelegat(int arrangementId, int delegatId)
    {
        return await _context.Delegat
            .Where(d => d.ArrangementId == arrangementId && d.Id == delegatId)
            .ProjectTo<DelegatDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();
    }

    [Authorize(Roles = "admin")]
    async Task<AdminDelegatDto?> IAdminDelegatService.HentDelegat(HentDelegatRequest request)
    {
        var (arrangementId, delgatId) = request;
        if (delgatId == null)
            throw new StemmeException("DelegatId er påkrevd");
        return await _context.Delegat
            .Where(d => d.ArrangementId == arrangementId && d.Id == delgatId)
            .ProjectTo<AdminDelegatDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();
    }
    [Authorize(Roles = "admin")]
    public async Task<AdminDelegatDto> OppdaterDelegat(DelegatInputModel dto)
    {
        var delegat = await _context.Delegat
            .Where(d => d.ArrangementId == dto.ArrangementId)
            .Where(d => d.Id == dto.Id)
            .FirstOrDefaultAsync();
        if (delegat == null)
            throw new StemmeException($"Fant ingen delegat med id {dto.Id} å oppdatere");

        if(dto.Delegatnummer.HasValue)
            delegat.Delegatnummer = dto.Delegatnummer.Value;
        delegat.Navn = dto.Navn;
        delegat.Gruppe = dto.Gruppe;
        delegat.Epost = dto.Epost;
        delegat.Telefon = dto.Telefon;
            
        await _context.SaveChangesAsync();
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

    async Task<ICollection<AdminDelegatDto>> IAdminDelegatService.HentDelegater(HentDelegatRequest request)
    {
        return await _context.Delegat
            .Where(d => d.ArrangementId == request.ArrangementId)
            .ProjectTo<AdminDelegatDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

    }

    public async Task<bool> IsValidNo(int arrangement, int number)
    {
        return await _context.Delegat
            .Where(d => d.ArrangementId == arrangement)
            .AllAsync(d => d.Delegatnummer != number);
    }
    
    [Authorize(Roles = "admin")]
    public async Task<AdminDelegatDto> RegistrerNyDelegat(DelegatInputModel dto)
    {
        if (!await IsValidNo(dto.ArrangementId, dto.Delegatnummer!.Value))
            throw new StemmeException("Delegatnummer er allerede brukt");

        var delegat = _mapper.Map<Delegat>(dto);
        delegat.ArrangementId = dto.ArrangementId;
        delegat.Delegatkode = _keyGenerator.GenerateKey(4);
        _context.Delegat.Add(delegat);
        await _context.SaveChangesAsync();
        return _mapper.Map<AdminDelegatDto>(delegat);
    }

    public async Task<DelegatDto?> ValiderKode(string delegatKode, CancellationToken cancellationToken = default)
    {
        delegatKode = delegatKode.ToUpper();
        var delegat = await _context.Delegat
            .Include(d => d.Arrangement)
            .Where(d => d.Delegatkode == delegatKode)
            .ProjectTo<DelegatDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
        return delegat;
    }
}