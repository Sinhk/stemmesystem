using System.Diagnostics.CodeAnalysis;
using AsyncKeyedLock;
using AutoMapper;
using Duende.IdentityServer.Extensions;
using Google.Rpc;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ProtoBuf.Grpc;
using Stemmesystem.Data;
using Stemmesystem.Data.Entities;
using DelegateEntity = Stemmesystem.Data.Entities.Delegate;
using Stemmesystem.Data.Repositories;
using Stemmesystem.Server.Data.Repositories;
using Stemmesystem.Shared;
using Stemmesystem.Shared.Interfaces;
using Stemmesystem.Shared.Models;
using Stemmesystem.Shared.Tools;
using ZiggyCreatures.Caching.Fusion;

namespace Stemmesystem.Server.Services;

[Authorize]
public class VoteService : IVoteService, IAdminVoteService
{
    private readonly IDelegateRepository _delegateRepository;
    private readonly StemmesystemContext _context;
    private readonly IKeyHasher _keyHasher;
    private readonly IArrangementRepository _arrangementRepository;
    private readonly NotificationManager _notificationManager;
    private readonly IMapper _mapper;
    private readonly IFusionCache _cache;

    private static readonly AsyncKeyedLocker<string> AsyncKeyedLocker = new();

    public VoteService(IDelegateRepository delegateRepository, IArrangementRepository arrangementRepository, StemmesystemContext context, IKeyHasher keyHasher, NotificationManager notificationManager, IMapper mapper, IFusionCache cache)
    {
        _delegateRepository = delegateRepository;
        _arrangementRepository = arrangementRepository;
        _context = context;

        _keyHasher = keyHasher;
        _notificationManager = notificationManager;
        _mapper = mapper;
        _cache = cache;
    }


    [DoesNotReturn]
    private static void ThrowError(string message)
    {
        var status = new Google.Rpc.Status
        {
            Code = (int)Code.FailedPrecondition,
            Message = message
        };
        throw status.ToRpcException();
    }
    public async Task<List<VoteDto>> CastVoteAsync(CastVoteRequest request, CallContext context = default)
    {
        var cancellationToken = context.CancellationToken;
        var delegateCode = (context.ServerCallContext?.GetHttpContext().User.GetSubjectId());
        if (delegateCode is null){
            ThrowError("Ingen delegatkode oppgitt");
        }
        var delegateEntity = await _delegateRepository.ValidateCode(delegateCode, cancellationToken);
        if (delegateEntity is null)
        {
            ThrowError($"Ugyldig delegatkode {delegateCode}");
        }
        if (delegateEntity.Present != true)
        {
            ThrowError("Du er ikke sjekket inn. Du må være sjekket inn for å kunne avgi stemme");
        }

        _context.Attach(delegateEntity);

        List<Vote>? votes, removed;
        Ballot? ballot;
        using (await AsyncKeyedLocker.LockAsync($"stemme-lock-{delegateEntity.Id}", cancellationToken))
        {
            ballot = await _context.Ballots
                .AsSingleQuery()
                .Include(v => v.Votes)
                .Include(v => v.VotedDelegates)
                .Include(v => v.Case)
                .SingleOrDefaultAsync(v => v.Id == request.BallotId, cancellationToken);

            if (ballot is null)
            {
                ThrowError($"Ugyldig votering {request.BallotId}");
            }

            if (ballot.Active == false)
            {
                ThrowError("Votering er ferdig eller har ikke startet enda");
            }

            (votes, removed) = ballot.RegisterVote(request.ChoiceIds, delegateEntity, delegateCode, _keyHasher);

            await _context.SaveChangesAsync(cancellationToken);
        }

        var notifier = _notificationManager.ForAdmin(ballot.Case.ArrangementId);

        if (removed != null && removed.Any())
            await Parallel.ForEachAsync(removed, cancellationToken, async (s, token) => await notifier.VoteRemoved(new VoteRemovedEvent(ballot.Id, s.Id), token));
        await Parallel.ForEachAsync(votes, cancellationToken, async (s, token) => await notifier.NewVote(new NewVoteEvent(ballot.Id, new VoteDto(s.Id, s.ChoiceId, null)), token));
        await notifier.Voted(new VotedEvent(ballot.Id, delegateEntity.Id), cancellationToken);
        var dto = _mapper.Map<List<VoteDto>>(votes);
        return dto;
    }

    [Authorize(Roles = "admin")]
    public async Task<GetResult<AdminBallotDto>> StartBallot(AdminBallotRequest request, CancellationToken cancellationToken = default)
    {
        var ballot = await _arrangementRepository.FindBallotAsync(request.ArrangementId, request.BallotId, cancellationToken);
        if (ballot == null)
            throw new VotingException("Fant ikke valgt votering");
        ballot.Start();

        await _context.SaveChangesAsync(cancellationToken);
        var e = new BallotStartedEvent(_mapper.Map<AdminBallotDto>(ballot));

        await _cache.RemoveAsync($"aktive-{request.ArrangementId}", token: cancellationToken);
        await _notificationManager.ForArrangement(request.ArrangementId).BallotStarted(e, cancellationToken);
        await _notificationManager.ForAdmin(request.ArrangementId).BallotStarted(e, cancellationToken);
        return new GetResult<AdminBallotDto>(_mapper.Map<AdminBallotDto>(ballot));
    }

    [Authorize(Roles = "admin")]
    public async Task<GetResult<AdminBallotDto>> StopBallot(AdminBallotRequest request, CancellationToken cancellationToken = default)
    {
        var ballot = await _arrangementRepository.FindBallotAsync(request.ArrangementId, request.BallotId, cancellationToken);
        if (ballot == null)
            throw new VotingException("Fant ikke valgt votering");

        await StopBallotInternal(ballot, request.ArrangementId, cancellationToken: cancellationToken);
        return new GetResult<AdminBallotDto>(_mapper.Map<AdminBallotDto>(ballot));
    }

    private async Task StopBallotInternal(Ballot ballot, int arrangementId, bool save = true, CancellationToken cancellationToken = default)
    {
        var presentCount = await _context.Cases
            .Where(s => s.Id == ballot.CaseId)
            .SelectMany(s => s.Arrangement.Delegates)
            .CountAsync(d => d.Present, cancellationToken);

        ballot.End(presentCount);
        if (save)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        await _cache.RemoveAsync($"aktive-{arrangementId}", token: cancellationToken);
        var voteDtos = ballot.Votes.Select(s => new VoteDto(s.Id, s.ChoiceId, s.DelegateId)).ToArray();
        var e = new BallotStoppedEvent(ballot.Id, ballot.EndTime!.Value, voteDtos);
        await _notificationManager.ForArrangement(arrangementId).BallotStopped(e, cancellationToken);
        await _notificationManager.ForAdmin(arrangementId).BallotStopped(e, cancellationToken);
    }


    public async Task<HasVotedResult> HasVoted(VoteRequest request, CallContext context = default)
    {
        var cancellationToken = context.CancellationToken;
        var delegateEntity = await FindDelegate(context, cancellationToken);
        var hasVoted = await _context.Ballots
            .Where(v => v.Id == request.BallotId)
            .SelectMany(v => v.VotedDelegates)
            .AnyAsync(s => s.Id == delegateEntity.Id, cancellationToken);

        List<VoteDto>? votes = null;
        if (hasVoted)
        {
            var tmp = await GetFindVoteQuery(_context, request.BallotId, delegateEntity, cancellationToken);
            votes = _mapper.Map<List<VoteDto>>(tmp);
        }
        var result = new HasVotedResult(hasVoted, votes);
        return result;
    }

    private async Task<DelegateEntity> FindDelegate(CallContext context, CancellationToken cancellationToken)
    {
        var delegateCode = context.ServerCallContext?.GetHttpContext().User.GetSubjectId();
        if (delegateCode == null)
            throw new VotingException("Fant ikke delegatkode");

        var delegateEntity = await _delegateRepository.ValidateCode(delegateCode, cancellationToken);
        if (delegateEntity == null)
            throw new VotingException($"Ugyldig delegat {delegateCode}");
        return delegateEntity;
    }

    public async Task<VoteDto> FindVote(int ballotId, Guid voteId, CancellationToken cancellationToken = default)
    {
        var vote = await _context.Ballots
            .Where(v => v.Id == ballotId)
            .SelectMany(v => v.Votes)
            .Where(s => s.Id == voteId)
            .FirstOrDefaultAsync(cancellationToken);

        return _mapper.Map<VoteDto>(vote);
    }

    public async Task<List<VoteDto>> FindVotes(int ballotId, string delegateCode, CancellationToken cancellationToken = default)
    {
        var votes = await GetFindVoteQuery(_context, ballotId, delegateCode, cancellationToken);
        return _mapper.Map<List<VoteDto>>(votes);
    }

    private async Task<List<Vote>> GetFindVoteQuery(StemmesystemContext context, int ballotId, string delegateCode, CancellationToken cancellationToken = default)
    {
        var delegateEntity = await _delegateRepository.ValidateCode(delegateCode, cancellationToken);
        if (delegateEntity == null)
            throw new VotingException($"Ugyldig delegat {delegateCode}");
        return await GetFindVoteQuery(context, ballotId, delegateEntity, cancellationToken);
    }

    private async Task<List<Vote>> GetFindVoteQuery(StemmesystemContext context, int ballotId, DelegateEntity delegateEntity, CancellationToken cancellationToken = default)
    {
        var ballot = await context.Ballots
            .Where(v => v.Id == ballotId)
            .Include(v => v.Votes)
            .FirstOrDefaultAsync(cancellationToken);
        if (ballot == null)
            throw new VotingException($"Votering med id {ballotId} ble ikke funnet");

        var votes = ballot.Secret
            ? ballot.Votes.Where(s => s.VoteHash != null && _keyHasher.VerifyHash(s.VoteHash, delegateEntity.DelegateCode)).ToList()
            : ballot.Votes.Where(s => s.DelegateId == delegateEntity.Id).ToList();

        return votes;
    }

    [Authorize(Roles = "admin")]
    public async Task<GetResult<AdminBallotDto>> LockBallot(AdminBallotRequest request, CancellationToken cancellationToken = default)
    {
        var (arrangementId, ballotId) = request;
        var ballot = await _arrangementRepository.FindBallotAsync(arrangementId, ballotId, cancellationToken);
        if (ballot == null)
            throw new VotingException("Fant ikke valgt votering");
        if (ballot.Active)
        {
            await StopBallotInternal(ballot, arrangementId, false, cancellationToken);
        }
        ballot.Lock();
        await _context.SaveChangesAsync(cancellationToken);

        await _notificationManager.ForAdmin(arrangementId).BallotLocked(new BallotLockedEvent(ballot.CaseId, ballot.Id, ballot.DelegatesPresent), cancellationToken);
        return new GetResult<AdminBallotDto>(_mapper.Map<AdminBallotDto>(ballot));
    }

    [Authorize(Roles = "admin")]
    public async Task<GetResult<AdminBallotDto>> PublishBallot(AdminBallotRequest request, CancellationToken cancellationToken = default)
    {
        var (arrangementId, ballotId) = request;
        var ballot = await _arrangementRepository.FindBallotAsync(arrangementId, ballotId, cancellationToken);
        if (ballot == null)
            throw new VotingException("Fant ikke valgt votering");
        ballot.Publish();
        await _context.SaveChangesAsync(cancellationToken);

        await _cache.RemoveAsync($"resultater-{request.ArrangementId}", token: cancellationToken);
        await _notificationManager.ForAdmin().BallotPublished(new BallotPublishedEvent(arrangementId, ballot.CaseId, ballot.Id), cancellationToken);
        await _notificationManager.ForArrangement(arrangementId).BallotPublished(new BallotPublishedEvent(arrangementId, ballot.CaseId, ballot.Id), cancellationToken);
        return new GetResult<AdminBallotDto>(_mapper.Map<AdminBallotDto>(ballot));
    }

    [Authorize(Roles = "admin")]
    public async Task<BallotInputModel> CopyBallot(AdminBallotRequest request, CancellationToken cancellationToken = default)
    {
        var (arrangementId, ballotId) = request;
        var ballot = await _arrangementRepository.FindBallotAsync(arrangementId, ballotId, cancellationToken);
        if (ballot == null)
            throw new VotingException("Fant ikke valgt votering");

        var copy = ballot.Copy();
        return _mapper.Map<BallotInputModel>(copy);
    }

    [Authorize(Roles = "admin")]
    public async Task ResetBallot(AdminBallotRequest request, CancellationToken cancellationToken = default)
    {
        var (arrangementId, ballotId) = request;
        var arrangement = await _arrangementRepository.GetArrangementAsync(arrangementId, cancellationToken);
        if (arrangement == null)
            return;
        _context.Attach(arrangement);
        var ballot = arrangement.FindBallot(ballotId);
        if (ballot == null) return;
        ballot.StartTime = null;
        ballot.EndTime = null;
        ballot.Locked = false;
        ballot.Published = false;
        await _context.SaveChangesAsync(cancellationToken);
    }
}