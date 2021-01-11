using Microsoft.EntityFrameworkCore;
using Stemmesystem.Data;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Stemmesystem.Web.Data
{
    public class StemmeService
    {
        private readonly IDelegatService delegatService;
        private readonly StemmesystemContext context;
        private readonly ArrangementService arrangementService;

        public StemmeService(IDelegatService delegatService, StemmesystemContext context, ArrangementService arrangementService)
        {
            this.delegatService = delegatService;
            this.context = context;
            this.arrangementService = arrangementService;
        }
        public async Task<Stemme> AvgiStemmeAsync(int voteringId, string delegatKode, Guid valgId, Stemme? gammelStemme = null, CancellationToken cancellationToken = default)
        {
            var delegat = await delegatService.ValiderKode(delegatKode, cancellationToken);
            if (delegat == null)
            {
                throw new StemmeException($"Ugyldig delegat {delegatKode}");
            }

            var votering = await context.Votering
                .SingleOrDefaultAsync(v=> v.Id == voteringId, cancellationToken: cancellationToken);

            if (votering == null)
            {
                throw new StemmeException($"Ugyldig votering {voteringId}");
            }

            var stemme = votering.RegistrerStemme(valgId, delegat, gammelStemme);
            await context.SaveChangesAsync(cancellationToken);
            return stemme;
        }

        public async Task<Votering?> AktivVotering(int arrangementId, CancellationToken cancellationToken = default)
        {
            var arrangement = await arrangementService.HentArrangementAsync(arrangementId, cancellationToken);
            return arrangement.AktivVotering();
        }
        public async Task StartVotering(int arrangementId, int voteringId, CancellationToken cancellationToken = default)
        {
            var arrangement = await arrangementService.HentArrangementAsync(arrangementId, cancellationToken);
            var aktiv = arrangement.AktivVotering();
            if(aktiv != null)
                throw new StemmeException($"Kun en votering kan være aktiv. {aktiv.Sak.Tittel}({aktiv.Tittel}) er aktiv");
            var votering = arrangement.FinnVotering(voteringId);
            if (votering == null)
                throw new StemmeException("Fant ikke valgt votering");
            votering.StartVotering();

            await context.SaveChangesAsync(cancellationToken);
            //EVENT 

        }

        public async Task<bool> HarStemmt(int voteringId, string delegatKode, CancellationToken cancellationToken = default)
        {
            var delegat = await delegatService.ValiderKode(delegatKode, cancellationToken);
            if (delegat == null)
            {
                throw new StemmeException($"Ugyldig delegat {delegatKode}");
            }

            return await context.Votering
                .Where(v=> v.Id == voteringId)
                .SelectMany(v => v.AvgitStemme)
                .AnyAsync(s => s.Id == delegat.Id, cancellationToken);
        }

        public async Task<Stemme?> FinnStemme(int voteringId, string delegatKode, CancellationToken cancellationToken = default)
        {
            var hemmelig = await context.Votering
                .Where(v => v.Id == voteringId)
                .Select(v => v.Hemmelig)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken);

            if (hemmelig == true)
                return null;

            var delegat = await delegatService.ValiderKode(delegatKode, cancellationToken);
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
