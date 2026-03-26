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
public class CaseService : ICaseService
{
    private readonly StemmesystemContext _context;
    private readonly IMapper _mapper;
    private readonly NotificationManager _notificationManager;
    private readonly IFusionCache _cache;

    public CaseService(IMapper mapper, NotificationManager notificationManager, StemmesystemContext context, IFusionCache cache)
    {
        _mapper = mapper;
        _notificationManager = notificationManager;
        _context = context;
        _cache = cache;
    }
    public async Task<CaseDto?> GetCase(CaseRequest request, CancellationToken cancellationToken = default)
    {
        var caseEntity = await _context.Arrangements
            .SelectMany(a => a.Cases)
            .Where(s => s.Id == request.CaseId)
            .ProjectTo<CaseDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        return caseEntity;
    }

    public async Task<BallotDto> GetBallot(GetBallotRequest request, CancellationToken cancellationToken = default)
    {
        var (caseId, ballotId) = request;
        return await _context.Arrangements
            .SelectMany(a => a.Cases)
            .Where(s => s.Id == caseId)
            .SelectMany(s=> s.Ballots)
            .Where(v=> v.Id == ballotId)
            .ProjectTo<BallotDto>(_mapper.ConfigurationProvider)
            .FirstAsync(cancellationToken: cancellationToken);
    }

    public async Task<ICollection<BallotDto>> GetBallots(GetBallotsRequest request, CancellationToken cancellationToken = default)
    {
        return await _context.Arrangements
            .AsSplitQuery()
            .SelectMany(a => a.Cases)
            .Where(s => s.Id == request.CaseId)
            .SelectMany(s=> s.Ballots)
            .ProjectTo<BallotDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsNumberUsed(int arrangementId, string? number)
    {
        return await _context.Arrangements
            .Where(a => a.Id == arrangementId)
            .SelectMany(a => a.Cases)
            .AnyAsync(s => s.Number == number);
    }

    [Authorize(Roles = "admin")]
    public async Task<SaveResult<CaseDto>> CreateCase(CaseInputModel model)
    {
        var errors = new Dictionary<string, List<string>>();
        var arrangement = await _context.Arrangements
            .Where(a => a.Id == model.ArrangementId)
            .Include(a => a.Cases)
            .FirstOrDefaultAsync();
        if (arrangement == null)
            throw new VotingException($"Arrangement med id {model.ArrangementId} ble ikke funnet");
            
        if ( await IsNumberUsed(model.ArrangementId, model.Number))
        {
            errors.Add(nameof(model.Number), new List<string>{"Saknummer er allerede brukt"});
            return new SaveResult<CaseDto>(null, errors);
        }
        
        var caseEntity = _mapper.Map<Case>(model);
        arrangement.Add(caseEntity);
        await _context.SaveChangesAsync();
        var dto = _mapper.Map<CaseDto>(caseEntity);
        return new SaveResult<CaseDto>(dto, errors);
    }

    [Authorize(Roles = "admin")]
    public async Task<CaseDto> UpdateCase(CaseInputModel model)
    {
        var caseEntity = await _context.Arrangements
            .Where(a => a.Id == model.ArrangementId)
            .SelectMany(a => a.Cases)
            .Where(s => s.Id == model.Id)
            .FirstAsync();

        caseEntity.Number = model.Number!;
        caseEntity.Title = model.Title!;
        caseEntity.Description = model.Description;

        await _context.SaveChangesAsync();
        return _mapper.Map<CaseDto>(caseEntity);
    }
    [Authorize(Roles = "admin")]
    public async Task<SaveResult<BallotDto>> CreateBallot(BallotInputModel model)
    {
        var errors = new Dictionary<string, List<string>>();
        var caseEntity = await _context.Cases
            .Where(a => a.Id == model.CaseId)
            .Include(a => a.Ballots)
            .FirstOrDefaultAsync();
        if (caseEntity == null)
            throw new VotingException($"Sak med id {model.CaseId} ble ikke funnet");

        var ballot = _mapper.Map<Ballot>(model);
        caseEntity.Add(ballot);
        await _context.SaveChangesAsync();
        var newBallot = _mapper.Map<AdminBallotDto>(ballot);
        await _notificationManager.ForAdmin(caseEntity.ArrangementId).NewBallot(new NewBallotEvent(newBallot));
        return new SaveResult<BallotDto>(newBallot, errors);
    }

    [Authorize(Roles = "admin")]
    public async Task<SaveResult<BallotDto>> UpdateBallot(BallotInputModel model)
    {
        var ballot = await _context.Ballots
            .Where(a => a.Id == model.Id)
            .FirstOrDefaultAsync();
        if (ballot == null)
            return SaveResult<BallotDto>.Error("Fant ikke votering å oppdatere");
        _mapper.Map(model, ballot);
        await _context.SaveChangesAsync();
        return SaveResult.Success(_mapper.Map<BallotDto>(ballot));
    }


        
    public async Task<ICollection<AdminCaseDto>> GetCases(CasesRequest request)
    {
        return await _context.Arrangements
            .Where(a => a.Id == request.ArrangementId)
            .AsSplitQuery()
            .SelectMany(a => a.Cases)
            .ProjectTo<AdminCaseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<GetResult<CaseInfoDto>> GetCaseInfo(CaseRequest request, CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrSetAsync<GetResult<CaseInfoDto>>($"sakinfo-{request.CaseId}", async token =>
        {
            var caseEntity = await _context.Arrangements
                .SelectMany(a => a.Cases)
                .Where(s => s.Id == request.CaseId)
                .ProjectTo<CaseInfoDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(token);
            return new GetResult<CaseInfoDto>(caseEntity);
        }, token: cancellationToken);
    }
}