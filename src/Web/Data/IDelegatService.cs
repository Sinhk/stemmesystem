using Microsoft.EntityFrameworkCore;
using Stemmesystem.Data;
using Stemmesystem.Data.Models;
using Stemmesystem.Tools;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Stemmesystem.Web.Data
{
    public interface IDelegatService
    {
        Task<Delegat?> ValiderKode(string delegatKode, CancellationToken cancellationToken = default);
        Delegat RegistrerNyDelegat(Arrangement arrangement, NyDelegatModel model);
    }

    public class DelegatService : IDelegatService
    {
        private readonly IDbContextFactory<StemmesystemContext> _contextFactory;
        private readonly IKeyGenerator _keyGenerator;

        public DelegatService(IKeyGenerator keyGenerator, IDbContextFactory<StemmesystemContext> contextFactory)
        {
            _keyGenerator = keyGenerator;
            _contextFactory = contextFactory;
        }

        public Delegat RegistrerNyDelegat(Arrangement arrangement, NyDelegatModel model)
        {
            var delegatkode = _keyGenerator.GenerateKey(4);
            if (arrangement.Delegater.Any(d => d.Delegatnummer == model.Nummer))
                throw new StemmeException($"Delegatnummer {model.Nummer} allerede registrert");

            return arrangement.NyDelegat(model, delegatkode);
        }

        public async Task<Delegat?> ValiderKode(string delegatKode, CancellationToken cancellationToken = default)
        {
            await using var context = _contextFactory.CreateDbContext();
            var delegat = await context.Delegat
                .Include(d => d.Arrangement)
                .Where(d => d.Delegatkode == delegatKode)
                .FirstOrDefaultAsync(cancellationToken);
            return delegat;
        }
    }
}