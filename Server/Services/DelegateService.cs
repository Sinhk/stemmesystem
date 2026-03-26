using System.Diagnostics.CodeAnalysis;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Duende.IdentityServer.Extensions;
using Google.Protobuf.WellKnownTypes;
using Google.Rpc;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ProtoBuf.Grpc;
using Stemmesystem.Data;
using Stemmesystem.Data.Entities;
using DelegateEntity = Stemmesystem.Data.Entities.Delegate;
using Stemmesystem.Shared;
using Stemmesystem.Shared.Interfaces;
using Stemmesystem.Shared.Models;
using Stemmesystem.Shared.SignalR;
using Stemmesystem.Shared.Tools;

namespace Stemmesystem.Server.Services;

[Authorize]
public class DelegateService : IDelegateService, IAdminDelegateService
{
    private readonly StemmesystemContext _context;
    private readonly IMapper _mapper;
    private readonly IKeyGenerator _keyGenerator;
    private readonly ILogger<DelegateService> _logger;
    private readonly NotificationManager _notificationManager;

    private int? _presentCount;

    public DelegateService(IKeyGenerator keyGenerator, StemmesystemContext context, IMapper mapper, ILogger<DelegateService> logger, NotificationManager notificationManager)
    {
        _keyGenerator = keyGenerator;
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _notificationManager = notificationManager;
    }

    public async Task<GetDelegateResult> GetDelegateInfo(CallContext context = default)
    {
        var user = context.ServerCallContext?.GetHttpContext().User;
        if (user == null || !user.IsInRole("Delegat"))
            return new GetDelegateResult(null);
        
        return new GetDelegateResult(await ValidateCode(user.GetSubjectId()));
    }
    
    public async Task<DelegateDto?> GetDelegateById(int arrangementId, int delegateId)
    {
        return await _context.Delegates
            .Where(d => d.ArrangementId == arrangementId && d.Id == delegateId)
            .ProjectTo<DelegateDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();
    }

    [Authorize(Roles = "admin")]
    async Task<AdminDelegateDto?> IAdminDelegateService.GetDelegate(GetDelegateRequest request)
    {
        var (arrangementId, delegateId) = request;
        if (delegateId == null)
            throw new VotingException("DelegatId er påkrevd");
        return await _context.Delegates
            .Where(d => d.ArrangementId == arrangementId && d.Id == delegateId)
            .ProjectTo<AdminDelegateDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();
    }
    [Authorize(Roles = "admin")]
    public async Task<AdminDelegateDto> UpdateDelegate(DelegateInputModel dto)
    {
        var delegateEntity = await _context.Delegates
            .Where(d => d.ArrangementId == dto.ArrangementId)
            .Where(d => d.Id == dto.Id)
            .FirstOrDefaultAsync();
        if (delegateEntity == null)
            throw new VotingException($"Fant ingen delegat med id {dto.Id} å oppdatere");

        if(dto.DelegateNumber.HasValue)
            delegateEntity.DelegateNumber = dto.DelegateNumber.Value;
        delegateEntity.Name = dto.Name;
        delegateEntity.Group = dto.Group;
        delegateEntity.Email = dto.Email;
        delegateEntity.Phone = dto.Phone;
            
        await _context.SaveChangesAsync();
        return _mapper.Map<AdminDelegateDto>(delegateEntity);
    }

    [Authorize(Roles = "admin")]
    public async Task<ICollection<DelegateDto>> GetAllDelegates(int arrangementId)
    {
        return await _context.Delegates
            .Where(d => d.ArrangementId == arrangementId)
            .ProjectTo<DelegateDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    async Task<ICollection<AdminDelegateDto>> IAdminDelegateService.GetDelegates(GetDelegateRequest request)
    {
        return await _context.Delegates
            .Where(d => d.ArrangementId == request.ArrangementId)
            .ProjectTo<AdminDelegateDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

    }

    public async Task<bool> IsValidNumber(int arrangement, int number)
    {
        return await _context.Delegates
            .Where(d => d.ArrangementId == arrangement)
            .AllAsync(d => d.DelegateNumber != number);
    }
    
    [Authorize(Roles = "admin")]
    public async Task<AdminDelegateDto> RegisterNewDelegate(DelegateInputModel dto)
    {
        if (!await IsValidNumber(dto.ArrangementId, dto.DelegateNumber!.Value))
            throw new VotingException("Delegatnummer er allerede brukt");

        var delegateEntity = _mapper.Map<DelegateEntity>(dto);
        delegateEntity.ArrangementId = dto.ArrangementId;
        delegateEntity.DelegateCode = _keyGenerator.GenerateKey(4);
        _context.Delegates.Add(delegateEntity);
        await _context.SaveChangesAsync();
        return _mapper.Map<AdminDelegateDto>(delegateEntity);
    }

    public async Task DeleteDelegate(DeleteDelegateRequest request, CancellationToken cancellationToken = default)
    {
        var delegateEntity = await _context.Delegates
            .Where(d => d.Id == request.DelegateId)
            .Include(d => d.VotedIn)
            .FirstOrDefaultAsync(cancellationToken);
        if (delegateEntity == null) ThrowUnknownDelegate(request.DelegateId);

        if (delegateEntity.VotedIn.Any())
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
                            new BadRequest.Types.FieldViolation { Field = "DelegateId", Description = $"Delegat {delegateEntity.DelegateNumber} har stemt i arrangementet og kan ikke slettes"}
                        }
                    })
                }
            };
            throw status.ToRpcException();
        }
        
        _context.Delegates.Remove(delegateEntity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task SetPresent(SetPresentRequest request, CancellationToken cancellationToken = default)
    {
        var delegateEntity = await _context.Delegates
            .Where(d => d.Id == request.DelegateId)
            .FirstOrDefaultAsync(cancellationToken);
        if (delegateEntity == null) ThrowUnknownDelegate(request.DelegateId);
        delegateEntity.Present = request.Present;
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Satt til stede for delegat {DelegatNavn} til {TilStede}", delegateEntity.Name, request.Present);

        if (_presentCount == null)
        {
            _presentCount = await _context.Arrangements
                .Where(a => a.Id == delegateEntity.ArrangementId)
                .SelectMany(a => a.Delegates)
                .CountAsync(d => d.Present, cancellationToken: cancellationToken);
        }
        else
        {
            _presentCount += request.Present ? 1 : -1;
        }
        
        await _notificationManager.ForAdmin()
            .PresentCountChanged(new PresentCountChangedEvent(delegateEntity.ArrangementId, _presentCount.Value), cancellationToken);
    }

    public async Task SetPresentForAll(SetPresentForAllRequest request, CancellationToken cancellationToken = default)
    {
        var delegates = await _context.Delegates
            .Where(d => d.ArrangementId == request.ArrangementId)
            .ToListAsync(cancellationToken);

        foreach (var delegateEntity in delegates)
        {
            delegateEntity.Present = request.Present;
        }
        
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Satt til stede for alle delegater i arrangement {ArrangementId} til {TilStede}", request.ArrangementId, request.Present);
        
        _presentCount = request.Present ? delegates.Count : 0;
        await _notificationManager.ForAdmin()
            .PresentCountChanged(new PresentCountChangedEvent(request.ArrangementId, _presentCount.Value), cancellationToken);
    }

    [DoesNotReturn]
    private static void ThrowUnknownDelegate(int delegateId)
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
                        new BadRequest.Types.FieldViolation { Field = "DelegateId", Description = $"Fant ingen delegat med id {delegateId}" }
                    }
                })
            }
        };
        throw status.ToRpcException();
    }

    public async Task<DelegateDto?> ValidateCode(string delegateCode, CancellationToken cancellationToken = default)
    {
        delegateCode = delegateCode.ToUpper();
        var delegateEntity = await _context.Delegates
            .Include(d => d.Arrangement)
            .Where(d => d.DelegateCode == delegateCode)
            .ProjectTo<DelegateDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
        return delegateEntity;
    }
}