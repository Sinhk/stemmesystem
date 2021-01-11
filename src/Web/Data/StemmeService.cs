using Microsoft.EntityFrameworkCore;
using Stemmesystem.Data;
using Stemmesystem.Tools;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Stemmesystem.Web.Data
{
    public class StemmeService
    {
        private readonly IDelegatService _delegatService;
        private readonly IDbContextFactory<StemmesystemContext> _contextFactory;
        private readonly IKeyGenerator _keyGenerator;
        private readonly IKeyHasher _keyHasher;
        private readonly ArrangementService _arrangementService;

        public StemmeService(IDelegatService delegatService, ArrangementService arrangementService, IDbContextFactory<StemmesystemContext> contextFactory, IKeyGenerator keyGenerator, IKeyHasher keyHasher)
        {
            _delegatService = delegatService;
            _arrangementService = arrangementService;
            _contextFactory = contextFactory;
            _keyGenerator = keyGenerator;
            _keyHasher = keyHasher;
        }
        public async Task<(Stemme stemme, string RevoteKey)> AvgiStemmeAsync(int voteringId, string delegatKode, Guid valgId, string? revoteKey = null, CancellationToken cancellationToken = default)
        {
            var delegat = await _delegatService.ValiderKode(delegatKode, cancellationToken);
            if (delegat == null)
            {
                throw new StemmeException($"Ugyldig delegat {delegatKode}");
            }
            await using var context = _contextFactory.CreateDbContext();
            context.Attach(delegat);
            var votering = await context.Votering
                .Include(v=> v.Stemmer)
                .Include(v => v.AvgitStemme)
                .SingleOrDefaultAsync(v => v.Id == voteringId, cancellationToken: cancellationToken);

            if (votering == null)
            {
                throw new StemmeException($"Ugyldig votering {voteringId}");
            }

            Stemme? gammelStemme = null;
            if (revoteKey != null)
            {
                gammelStemme = votering.Stemmer.FirstOrDefault(s =>
                {
                    var hash = context.Entry(s).Property("RevoteKey").CurrentValue as string;
                    return _keyHasher.VerifyHashedKey(hash, revoteKey);
                });

                if (gammelStemme == null)
                    throw new StemmeException("Ugyldig endring av stemme");
            }
            else if (votering.AvgitStemme.Any(d => d.Id == delegat.Id))
            {
                if(votering.Hemmelig)
                    throw new StemmeException("Delegat har allerede stemmt");
                gammelStemme = votering.Stemmer.FirstOrDefault(s => s.DelegatId == delegat.Id);
                if (gammelStemme == null)
                    throw new StemmeException("Ugyldig endring av stemme");
            }

            var stemme = votering.RegistrerStemme(valgId, delegat, gammelStemme);
            var newKey = _keyGenerator.GenerateKey(20);
            var revoteHash = _keyHasher.CalculateHash(newKey);

            context.Entry(stemme).Property("RevoteKey").CurrentValue = revoteHash;
            await context.SaveChangesAsync(cancellationToken);

            return (stemme, newKey);
        }

        public async Task<Votering?> AktivVotering(int arrangementId, CancellationToken cancellationToken = default)
        {
            var arrangement = await _arrangementService.HentArrangementAsync(arrangementId, cancellationToken);
            return arrangement.AktivVotering();
        }
        public async Task StartVotering(int arrangementId, int voteringId, CancellationToken cancellationToken = default)
        {
            await using var context = _contextFactory.CreateDbContext();
            var arrangement = await _arrangementService.HentArrangementAsync(arrangementId, cancellationToken);
            context.Attach(arrangement);
            var aktiv = arrangement.AktivVotering();
            if (aktiv != null)
                throw new StemmeException($"Kun en votering kan være aktiv. {aktiv.Sak.Tittel}({aktiv.Tittel}) er aktiv");
            var votering = arrangement.FinnVotering(voteringId);
            if (votering == null)
                throw new StemmeException("Fant ikke valgt votering");
            votering.StartVotering();

            await context.SaveChangesAsync(cancellationToken);
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

        public async Task<Stemme?> FinnStemme(int voteringId, string delegatKode, CancellationToken cancellationToken = default)
        {
            await using var context = _contextFactory.CreateDbContext();
            var hemmelig = await context.Votering
                .Where(v => v.Id == voteringId)
                .Select(v => v.Hemmelig)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken);

            if (hemmelig == true)
                return null;

            var delegat = await _delegatService.ValiderKode(delegatKode, cancellationToken);
            if (delegat == null)
            {
                throw new StemmeException($"Ugyldig delegat {delegatKode}");
            }

            return await context.Votering
                .Where(v => v.Id == voteringId)
                .SelectMany(v => v.Stemmer)
                .FirstOrDefaultAsync(s => s.Delegat!.Id == delegat.Id, cancellationToken: cancellationToken);
        }
    }
}
