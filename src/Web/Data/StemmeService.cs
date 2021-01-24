using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Stemmesystem.Data;
using Stemmesystem.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Stemmesystem.Web.Data
{
    public class StemmeService
    {
        private readonly IDelegatService _delegatService;
        private readonly IDbContextFactory<StemmesystemContext> _contextFactory;
        private readonly IKeyHasher _keyHasher;
        private readonly ArrangementService _arrangementService;
        private readonly NotificationManager _notificationManager;

        public StemmeService(IDelegatService delegatService, ArrangementService arrangementService, IDbContextFactory<StemmesystemContext> contextFactory, IKeyGenerator keyGenerator, IKeyHasher keyHasher, NotificationManager notificationManager)
        {
            _delegatService = delegatService;
            _arrangementService = arrangementService;
            _contextFactory = contextFactory;
            _keyHasher = keyHasher;
            _notificationManager = notificationManager;
        }

        public async Task<(Stemme stemme, string RevoteKey)> AvgiStemmeAsync(int voteringId, string delegatkode, Guid valgId, string? revoteKey = null, CancellationToken cancellationToken = default)
        {
            var (stemmer, key) = await AvgiStemmeAsync(voteringId, delegatkode, new[] {valgId}, revoteKey, cancellationToken);
            if (stemmer.Count > 1)
                throw new StemmeException("Noe gikk galt.");
            return (stemmer.First(), key);
        }

        //TODO: Ditch the revoteKey thing and just use delegatkode
        public async Task<(List<Stemme> stemmer, string RevoteKey)> AvgiStemmeAsync(int voteringId, string delegatkode, IEnumerable<Guid> valgIder, string? revoteKey = null, CancellationToken cancellationToken = default)
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
                .SingleOrDefaultAsync(v => v.Id == voteringId, cancellationToken: cancellationToken);

            if (votering == null)
            {
                throw new StemmeException($"Ugyldig votering {voteringId}");
            }
            
            if (votering.Aktiv == false)
            {
                throw new StemmeException($"Votering er ferdig eller har ikke startet enda");
            }

            var (stemmer, newKey) = votering.RegistrerStemme(valgIder, delegat, revoteKey, _keyHasher, _notificationManager);
            
            await context.SaveChangesAsync(cancellationToken);
            return (stemmer, newKey);
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
            var q = await GetFinnStemmeQuery(context, voteringId, delegatKode, cancellationToken);
            if (q == null) return new List<Stemme>();
            return await q.ToListAsync(cancellationToken);
        }
        public async Task<Stemme?> FinnStemme(int voteringId, string delegatKode, CancellationToken cancellationToken = default)
        {
            await using var context = _contextFactory.CreateDbContext();
            var q = await GetFinnStemmeQuery(context, voteringId, delegatKode, cancellationToken);
            if (q == null) return null;
            return await q.FirstOrDefaultAsync(cancellationToken);
        }

        private async Task<IQueryable<Stemme>?> GetFinnStemmeQuery(StemmesystemContext context, int voteringId, string delegatKode, CancellationToken cancellationToken = default)
        {
            var hemmelig = await context.Votering
                .Where(v => v.Id == voteringId)
                .Select(v => v.Hemmelig)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken);

            if (hemmelig == true)
                return null;

            var delegat = await _delegatService.ValiderKode(delegatKode, cancellationToken);
            if (delegat == null)
                throw new StemmeException($"Ugyldig delegat {delegatKode}");

            return context.Votering
                .Where(v => v.Id == voteringId)
                .SelectMany(v => v.Stemmer)
                .Where(s => s.Delegat!.Id == delegat.Id);
        }
    }
}
