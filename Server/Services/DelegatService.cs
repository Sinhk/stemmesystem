using System.Diagnostics.CodeAnalysis;
using Google.Protobuf.WellKnownTypes;
using Google.Rpc;
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
    private static readonly int KeyLength = 6;
    private readonly StemmesystemContext _context;
    private readonly IKeyGenerator _keyGenerator;
    private readonly ILogger<DelegatService> _logger;
    private readonly NotificationManager _notificationManager;

    private int? _tilstedeCount;

    public DelegatService(IKeyGenerator keyGenerator, StemmesystemContext context, ILogger<DelegatService> logger, NotificationManager notificationManager)
    {
        _keyGenerator = keyGenerator;
        _context = context;
        _logger = logger;
        _notificationManager = notificationManager;
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
            .ToDtos()
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
            .ToAdminDtos()
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
        return delegat.ToAdminDto();
    }

    [Authorize(Roles = "admin")]
    public async Task<ICollection<DelegatDto>> HentDelegater(int arrangementId)
    {
        return await _context.Delegat
            .Where(d => d.ArrangementId == arrangementId)
            .ToDtos()
            .ToListAsync();
    }

    async Task<ICollection<AdminDelegatDto>> IAdminDelegatService.HentDelegater(HentDelegatRequest request)
    {
        return await _context.Delegat
            .Where(d => d.ArrangementId == request.ArrangementId)
            .ToAdminDtos()
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
        ArgumentNullException.ThrowIfNull(dto.Delegatnummer);
        var delegatnummer = dto.Delegatnummer.Value;
        if (!await IsValidNo(dto.ArrangementId, delegatnummer))
            throw new StemmeException("Delegatnummer er allerede brukt");

        var delegatkode = _keyGenerator.GenerateKey(KeyLength);
        var delegat = new Delegat(delegatnummer, dto.Navn, delegatkode)
            {
                ArrangementId = dto.ArrangementId,
                Gruppe = dto.Gruppe,
                Epost = dto.Epost,
                Telefon = dto.Telefon
            };

        _context.Delegat.Add(delegat);
        await _context.SaveChangesAsync();
        return delegat.ToAdminDto();
    }

    public async Task SlettDelegat(SlettDelegatRequest request, CancellationToken cancellationToken = default)
    {
        var delegat = await _context.Delegat
            .Where(d => d.Id == request.DelegatId)
            .Include(d => d.HarStemmtI)
            .FirstOrDefaultAsync(cancellationToken);
        if (delegat == null) ThrowUkjentDelegat(request.DelegatId);

        if (delegat.HarStemmtI.Any())
        {
            var status = new Google.Rpc.Status
            {
                Code = (int)Code.FailedPrecondition,
                Details =
                {
                    Any.Pack(new BadRequest
                    {
                        FieldViolations =
                        {
                            new BadRequest.Types.FieldViolation { Field = "DelegatId", Description = $"Delegat {delegat.Delegatnummer} har stemt i arrangementet og kan ikke slettes"}
                        }
                    })
                }
            };
            throw status.ToRpcException();
        }
        
        _context.Delegat.Remove(delegat);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task SetTilStede(SetTilstedeRequest request, CancellationToken cancellationToken = default)
    {
        var delegat = await _context.Delegat
            .Where(d => d.Id == request.DelegatId)
            .FirstOrDefaultAsync(cancellationToken);
        if (delegat == null) ThrowUkjentDelegat(request.DelegatId);
        delegat.TilStede = request.TilStede;
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Satt til stede for delegat {DelegatNavn} til {TilStede}", delegat.Navn, request.TilStede);

        if (_tilstedeCount == null)
        {
            _tilstedeCount =  await _context.Arrangement
                .Where(a => a.Id == delegat.ArrangementId)
                .SelectMany(a => a.Delegater)
                .CountAsync(d => d.TilStede, cancellationToken: cancellationToken);
        }
        else
        {
            _tilstedeCount += request.TilStede ? 1 : -1;
        }
        
        await _notificationManager.ForAdmin()
            .TilstedeCountChanged(new TilstedeCountChangedEvent(delegat.ArrangementId, _tilstedeCount.Value), cancellationToken);
    }

    public async Task SetTilStedeForAll(SetTilstedeForAllRequest request, CancellationToken cancellationToken = default)
    {
        var delegater = await _context.Delegat
            .Where(d => d.ArrangementId == request.ArrangementId)
            .ToListAsync(cancellationToken);

        foreach (var delegat in delegater)
        {
            delegat.TilStede = request.TilStede;
        }
        
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Satt til stede for alle delegater i arrangement {ArrangementId} til {TilStede}", request.ArrangementId, request.TilStede);
        
        _tilstedeCount = request.TilStede ? delegater.Count : 0;
        await _notificationManager.ForAdmin()
            .TilstedeCountChanged(new TilstedeCountChangedEvent(request.ArrangementId, _tilstedeCount.Value), cancellationToken);
    }

    [DoesNotReturn]
    private static void ThrowUkjentDelegat(int DelegatId)
    {
        var status = new Google.Rpc.Status
        {
            Code = (int)Code.NotFound,
            Details =
            {
                Any.Pack(new BadRequest
                {
                    FieldViolations =
                    {
                        new BadRequest.Types.FieldViolation { Field = "DelegatId", Description = $"Fant ingen delegat med id {DelegatId}" }
                    }
                })
            }
        };
        throw status.ToRpcException();
    }

    public async Task<DelegatDto?> ValiderKode(string delegatKode, CancellationToken cancellationToken = default)
    {
        delegatKode = delegatKode.ToUpper();
        var delegat = await _context.Delegat
            .Include(d => d.Arrangement)
            .Where(d => d.Delegatkode == delegatKode)
            .ToDtos()
            .FirstOrDefaultAsync(cancellationToken);
        return delegat;
    }
}