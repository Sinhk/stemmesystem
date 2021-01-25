using System.Collections.Generic;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Stemmesystem.Data;
using Stemmesystem.Data.Models;
using Stemmesystem.Tools;
using Stemmesystem.Web.Models;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Stemmesystem.Web.Data
{
    public interface IDelegatService
    {
        Task<Delegat?> ValiderKode(string delegatKode, CancellationToken cancellationToken = default);
        Delegat RegistrerNyDelegat(Arrangement arrangement, NyDelegatModel model);
        Task<bool> IsValidNo(int arrangement, int number);

        Task<DelegatModel> RegistrerNyDelegat(int arrangementId, DelegatModel model);
        Task<DelegatModel?> HentDelegat(int arrangementId, int delegatId);
        Task<DelegatModel> OppdaterDelegat(int arrangementId, DelegatModel model);
        Task<ICollection<Delegat>> HentDelegater(int arrangementId);
    }

    public class DelegatService : IDelegatService
    {
        private readonly IDbContextFactory<StemmesystemContext> _contextFactory;
        private readonly IMapper _mapper;
        private readonly IKeyGenerator _keyGenerator;

        public DelegatService(IKeyGenerator keyGenerator, IDbContextFactory<StemmesystemContext> contextFactory, IMapper mapper)
        {
            _keyGenerator = keyGenerator;
            _contextFactory = contextFactory;
            _mapper = mapper;
        }

        public async Task<DelegatModel?> HentDelegat(int arrangementId, int delegatId)
        {
            await using var context = _contextFactory.CreateDbContext();
            return await context.Delegat
                .Where(d => d.ArrangementId == arrangementId && d.Id == delegatId)
                .ProjectTo<DelegatModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
        }

        public async Task<DelegatModel> OppdaterDelegat(int arrangementId, DelegatModel model)
        {
            await using var context = _contextFactory.CreateDbContext();
            var delegat = await context.Delegat
                .Where(d => d.ArrangementId == arrangementId)
                .Where(d => d.Id == model.Id)
                .FirstOrDefaultAsync();
            if (delegat == null)
                throw new StemmeException($"Fant ingen delegat med id {model.Id} å oppdatere");

            delegat.Navn = model.Navn;
            delegat.Epost = model.Epost;
            delegat.Telefon = model.Telefon;
            
            await context.SaveChangesAsync();
            return _mapper.Map<DelegatModel>(model);
        }

        public async Task<ICollection<Delegat>> HentDelegater(int arrangementId)
        {
            await using var context = _contextFactory.CreateDbContext();
            return await context.Delegat
                .Where(d => d.ArrangementId == arrangementId)
                //.ProjectTo<DelegatModel>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<bool> IsValidNo(int arrangement, int number)
        {
            await using var context = _contextFactory.CreateDbContext();
            return await context.Delegat
                .Where(d => d.ArrangementId == arrangement)
                .AllAsync(d => d.Delegatnummer != number);
        }

        public Delegat RegistrerNyDelegat(Arrangement arrangement, NyDelegatModel model)
        {
            var delegatkode = _keyGenerator.GenerateKey(4);
            if (arrangement.Delegater.Any(d => d.Delegatnummer == model.Nummer))
                throw new StemmeException($"Delegatnummer {model.Nummer} allerede registrert");

            return arrangement.NyDelegat(model, delegatkode);
        }

        public async Task<DelegatModel> RegistrerNyDelegat(int arrangementId, DelegatModel model)
        {
            var delegat = _mapper.Map<Delegat>(model);
            await using var context = _contextFactory.CreateDbContext();
            delegat.ArrangementId = arrangementId;
            context.Delegat.Add(delegat);
            await context.SaveChangesAsync();
            return _mapper.Map<DelegatModel>(delegat);
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