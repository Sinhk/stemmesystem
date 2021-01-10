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
        private readonly StemmesystemContext _context;
        private readonly IKeyGenerator _keyGenerator;

        public DelegatService(StemmesystemContext context, IKeyGenerator keyGenerator)
        {
            _context = context;
            _keyGenerator = keyGenerator;
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
            var delegat = await _context.Delegat.Where(d => d.DelegatKode == delegatKode).FirstOrDefaultAsync(cancellationToken);
            return delegat;
        }
    }
}