using Microsoft.EntityFrameworkCore;
using Stemmesystem.Data;
using Stemmesystem.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Stemmesystem.Web.Models;

namespace Stemmesystem.Web.Data
{
    public class StemmeService
    {
        private readonly IDelegatService _delegatService;
        private readonly IDbContextFactory<StemmesystemContext> _contextFactory;
        private readonly IKeyHasher _keyHasher;
        private readonly ArrangementService _arrangementService;
        private readonly NotificationManager _notificationManager;
        private readonly IMapper _mapper;

        public StemmeService(IDelegatService delegatService, ArrangementService arrangementService, IDbContextFactory<StemmesystemContext> contextFactory, IKeyHasher keyHasher, NotificationManager notificationManager, IMapper mapper)
        {
            _delegatService = delegatService;
            _arrangementService = arrangementService;
            _contextFactory = contextFactory;
            _keyHasher = keyHasher;
            _notificationManager = notificationManager;
            _mapper = mapper;
        }

        public async Task<Stemme> AvgiStemmeAsync(int voteringId, string delegatkode, Guid valgId, CancellationToken cancellationToken = default)
        {
            var stemmer = await AvgiStemmeAsync(voteringId, delegatkode, new[] {valgId}, cancellationToken);
            if (stemmer.Count > 1)
                throw new StemmeException("Noe gikk galt.");
            return stemmer.First();
        }

        public async Task<List<Stemme>> AvgiStemmeAsync(int voteringId, string delegatkode, IEnumerable<Guid> valgIder, CancellationToken cancellationToken = default)
        {
            var delegat = await _delegatService.ValiderKode(delegatkode, cancellationToken);
            if (delegat == null)
                throw new StemmeException($"Ugyldig delegat {delegatkode}");

            await using var context = _contextFactory.CreateDbContext();
            context.Attach(delegat);
            var votering = await context.Votering
                .AsSingleQuery()
                .Include(v=> v.Stemmer)
                .Include(v => v.AvgitStemme)
                .Include(v=> v.Sak)
                .SingleOrDefaultAsync(v => v.Id == voteringId, cancellationToken);

            if (votering == null)
            {
                throw new StemmeException($"Ugyldig votering {voteringId}");
            }
            
            if (votering.Aktiv == false)
            {
                throw new StemmeException("Votering er ferdig eller har ikke startet enda");
            }

            var stemmer = votering.RegistrerStemme(valgIder, delegat, delegatkode, _keyHasher, _notificationManager);
            
            await context.SaveChangesAsync(cancellationToken);
            return stemmer;
        }

        public async Task StartVotering(int arrangementId, int voteringId, CancellationToken cancellationToken = default)
        {
            await using var context = _contextFactory.CreateDbContext();
            var arrangement = await _arrangementService.HentArrangementAsync(arrangementId, cancellationToken);
            if(arrangement == null)
                return;
            context.Attach(arrangement);
            var votering = arrangement.FinnVotering(voteringId);
            if (votering == null)
                throw new StemmeException("Fant ikke valgt votering");
            votering.StartVotering();

            await context.SaveChangesAsync(cancellationToken);
            
            _notificationManager.ForArrangement(arrangementId).OnVoteringStartet(new(votering.Id));
        }

        public async Task StoppVotering(int arrangementId, int voteringId, CancellationToken cancellationToken = default)
        {
            await using var context = _contextFactory.CreateDbContext();
            var arrangement = await _arrangementService.HentArrangementAsync(arrangementId, cancellationToken);
            if(arrangement == null)
                return;
            context.Attach(arrangement);
            var votering = arrangement.FinnVotering(voteringId);
            if (votering == null)
                throw new StemmeException("Fant ikke valgt votering");
            votering.AvsluttVotering();
            await context.SaveChangesAsync(cancellationToken);
            
            _notificationManager.ForArrangement(arrangementId).OnVoteringStoppet(new(votering.Id));
        }

        public async Task<bool> HarStemmt(int voteringId, string delegatKode, CancellationToken cancellationToken = default)
        {
            var delegat = await _delegatService.ValiderKode(delegatKode, cancellationToken);
            if (delegat == null)
            {
                throw new StemmeException($"Ugyldig delegat {delegatKode}");
            }
            await using var context = _contextFactory.CreateDbContext();
            return await context.Votering
                .Where(v => v.Id == voteringId)
                .SelectMany(v => v.AvgitStemme)
                .AnyAsync(s => s.Id == delegat.Id, cancellationToken);
        }

        public async Task<List<Stemme>> FinnStemmer(int voteringId, string delegatKode, CancellationToken cancellationToken = default)
        {
            await using var context = _contextFactory.CreateDbContext();
            return await GetFinnStemmeQuery(context, voteringId, delegatKode, cancellationToken);
        }

        private async Task<List<Stemme>> GetFinnStemmeQuery(StemmesystemContext context, int voteringId, string delegatkode, CancellationToken cancellationToken = default)
        {
            var delegat = await _delegatService.ValiderKode(delegatkode, cancellationToken);
            if (delegat == null)
                throw new StemmeException($"Ugyldig delegat {delegatkode}");
            
            var votering = await context.Votering
                .Where(v => v.Id == voteringId)
                .Include(v=> v.Stemmer)
                .FirstOrDefaultAsync(cancellationToken);

            var stemmer = votering.Hemmelig
                ? votering.Stemmer.Where(s => _keyHasher.VerifyHash(s.StemmeHash, delegatkode)).ToList()
                : votering.Stemmer.Where(s => s.DelegatId == delegat.Id).ToList();

            return stemmer;
        }

        public async Task LukkVotering(int arrangementId, int voteringId, CancellationToken cancellationToken = default)
        {
            await using var context = _contextFactory.CreateDbContext();
            var arrangement = await _arrangementService.HentArrangementAsync(arrangementId, cancellationToken);
            if(arrangement == null)
                return;
            context.Attach(arrangement);
            var votering = arrangement.FinnVotering(voteringId);
            if (votering == null)
                throw new StemmeException("Fant ikke valgt votering");
            if (votering.Aktiv)
            {
                votering.AvsluttVotering();
                _notificationManager.ForArrangement(arrangementId).OnVoteringStoppet(new VoteringStoppetEvent(votering.Id));
            }
            votering.LukkVotering();
            await context.SaveChangesAsync(cancellationToken);
            
            _notificationManager.ForArrangement(arrangementId).OnVoteringLukket(new VoteringLukketEvent(votering.Id));
        }
        
        public async Task PubliserVotering(int arrangementId, int voteringId, CancellationToken cancellationToken = default)
        {
            await using var context = _contextFactory.CreateDbContext();
            var arrangement = await _arrangementService.HentArrangementAsync(arrangementId, cancellationToken);
            if(arrangement == null)
                return;
            context.Attach(arrangement);
            var votering = arrangement.FinnVotering(voteringId);
            if (votering == null)
                throw new StemmeException("Fant ikke valgt votering");
            votering.PubliserVotering();
            await context.SaveChangesAsync(cancellationToken);
            
            _notificationManager.ForArrangement(arrangementId).OnVoteringPublisert(new VoteringPublisertEvent(votering.Id, votering));
        }

        public async Task<VoteringModel> KopierVotering(int arrangementId, int voteringId, CancellationToken cancellationToken = default)
        {
            await using var context = _contextFactory.CreateDbContext();
            var arrangement = await _arrangementService.HentArrangementAsync(arrangementId, cancellationToken);
            if(arrangement == null)
                throw new StemmeException("Fant ikke valgt arrangement");
            context.Attach(arrangement);
            var votering = arrangement.FinnVotering(voteringId);
            if (votering == null)
                throw new StemmeException("Fant ikke valgt votering");
            
            var kopi = votering.Kopier();
            return _mapper.Map<VoteringModel>(kopi);
        }
    }
}
