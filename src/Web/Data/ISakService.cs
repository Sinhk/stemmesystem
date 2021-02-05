using System;
using System.Collections.Generic;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Stemmesystem.Data;
using Stemmesystem.Web.Models;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Stemmesystem.Web.Data
{
    public interface ISakService
    {
        Task<SakModel?> HentSak(int sakId, CancellationToken cancellationToken = default);
        Task<bool> ErNummerBrukt(int arrangementId, string? nummer);
        Task<SakModel> LagreNySak(int arrangementId, SakModel sak);
        Task<SakModel> OppdaterSak(int arrangementId, SakModel sak);
        Task<VoteringModel?> HentVotering(int sakId, int voteringId, CancellationToken cancellationToken = default);
        Task<VoteringModel> LagreNyVotering(int sakId, VoteringModel votering);
        Task<VoteringModel> OppdaterVotering(VoteringModel votering);
        Task<ICollection<Sak>> HentSaker(int arrangementId);
    }

    public class SakService : ISakService
    {
        private readonly IDbContextFactory<StemmesystemContext> _dbContextFactory;
        private readonly IMapper _mapper;
        private readonly NotificationManager _notificationManager;

        public SakService(IDbContextFactory<StemmesystemContext> dbContextFactory,  IMapper mapper, NotificationManager notificationManager)
        {
            _dbContextFactory = dbContextFactory;
            _mapper = mapper;
            _notificationManager = notificationManager;
        }
        public async Task<SakModel?> HentSak(int sakId, CancellationToken cancellationToken = default)
        {
            await using var db = _dbContextFactory.CreateDbContext();
            var sak = await db.Arrangement
                .SelectMany(a => a.Saker)
                .Where(s => s.Id == sakId)
                .ProjectTo<SakModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken);
            return sak;
        }

        public async Task<VoteringModel?> HentVotering(int sakId, int voteringId, CancellationToken cancellationToken = default)
        {
            await using var db = _dbContextFactory.CreateDbContext();
            return await db.Arrangement
                .SelectMany(a => a.Saker)
                .Where(s => s.Id == sakId)
                .SelectMany(s=> s.Voteringer)
                .Where(v=> v.Id == voteringId)
                .ProjectTo<VoteringModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        }

        public async Task<bool> ErNummerBrukt(int arrangementId, string? nummer)
        {
            await using var db = _dbContextFactory.CreateDbContext();
            return await db.Arrangement
                .Where(a => a.Id == arrangementId)
                .SelectMany(a => a.Saker)
                .AnyAsync(s => s.Nummer == nummer);
        }

        public async Task<SakModel> LagreNySak(int arrangementId, SakModel model)
        {
            await using var db = _dbContextFactory.CreateDbContext();
            var arrangement = await db.Arrangement
                .Where(a => a.Id == arrangementId)
                .Include(a => a.Saker)
                .FirstOrDefaultAsync();
            var sak = _mapper.Map<Sak>(model);
            arrangement.LeggTil(sak);
            await db.SaveChangesAsync();
            return _mapper.Map<SakModel>(sak);
        }

        public async Task<SakModel> OppdaterSak(int arrangementId, SakModel model)
        {
            await using var db = _dbContextFactory.CreateDbContext();
            var sak = await db.Arrangement
                .Where(a => a.Id == arrangementId)
                .SelectMany(a => a.Saker)
                .Where(s => s.Id == model.Id)
                .FirstAsync();

            sak.Nummer = model.Nummer!;
            sak.Tittel = model.Tittel!;
            sak.Beskrivelse = model.Beskrivelse;

            await db.SaveChangesAsync();
            return _mapper.Map<SakModel>(sak);
        }

        public async Task<VoteringModel> LagreNyVotering(int sakId, VoteringModel model)
        {
            await using var db = _dbContextFactory.CreateDbContext();
            var sak = await db.Sak
                .Where(a => a.Id == sakId)
                .Include(a => a.Voteringer)
                .FirstOrDefaultAsync();
            var votering = _mapper.Map<Votering>(model);
            sak.LeggTil(votering);
            await db.SaveChangesAsync();
            _notificationManager.ForArrangement(sak.ArrangementId).OnNyVotering(new NyVoteringEvent(votering));
            return _mapper.Map<VoteringModel>(votering);
        }

        public async Task<VoteringModel> OppdaterVotering(VoteringModel model)
        {
            await using var db = _dbContextFactory.CreateDbContext();
            var votering = await db.Votering
                .Where(a => a.Id == model.Id)
                .FirstOrDefaultAsync();
            _mapper.Map(model, votering);
            await db.SaveChangesAsync();
            return _mapper.Map<VoteringModel>(votering);
        }

        public async Task<ICollection<Sak>> HentSaker(int arrangementId)
        {
            await using var db = _dbContextFactory.CreateDbContext();
            return await db.Arrangement
                .Where(a => a.Id == arrangementId)
                .AsSingleQuery()
                .SelectMany(a => a.Saker)
                .Include(s => s.Voteringer)
                .ThenInclude(v=> v.Stemmer)
                .Include(s => s.Voteringer)
                .ThenInclude(v=> v.AvgitStemme)
                //.ProjectTo<SakModel>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }
    }
}