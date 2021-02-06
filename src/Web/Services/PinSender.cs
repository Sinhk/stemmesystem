using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Stemmesystem.Data;
using Stemmesystem.Web.Data;

namespace Stemmesystem.Web.Services
{
    public interface IPinSender
    {
        Task SendEmail(int delegatId, string baseUri);
        Task<bool> SendSms(int delegatId, string baseUri);
    }
    
    public class PinSender : IPinSender
    {
        private readonly ISmsSender _smsSender;
        private readonly IEpostSender _emailSender;
        private readonly IDbContextFactory<StemmesystemContext> _contextFactory;

        public PinSender(ISmsSender smsSender, IEpostSender emailSender, IDbContextFactory<StemmesystemContext> contextFactory)
        {
            _smsSender = smsSender;
            _emailSender = emailSender;
            _contextFactory = contextFactory;
        }

        public async Task SendEmail(int delegatId, string baseUri)
        {
            await using var context = _contextFactory.CreateDbContext();
            var delegat = await context.Delegat
                .Where(d => d.Id == delegatId)
                .Include(d=> d.Arrangement)
                .FirstOrDefaultAsync();
            if (delegat == null)
                throw new StemmeException($"Fant ikke delegat med id{delegatId}");
            if (string.IsNullOrEmpty(delegat.Epost))
                throw new StemmeException($"Delegat {delegat.Delegatnummer} ({delegat.Navn}) har ikke e-postadresse");
            
            //var msg = $"Hei. {delegat.Navn} har PIN-kode {delegat.Delegatkode} i Romsdal og Nordmøre krets sitt system for elektronisk avstemning. Bruk PIN-koden på {baseUri} for å avgi din stemme i de sakene der du har stemmerett. Du kan også bruke lenken: {baseUri}/pin/{delegat.Delegatkode}";
            //await _emailSender.SendEmailAsync(delegat.Epost,$"PIN-kode til {delegat.Arrangement.Navn}",msg);
            await _emailSender.SendEmailTemplateAsync(delegat.Epost, delegat.Navn, "d-c4f5f9ba460546779394bce9d3ab97b8", new {navn = delegat.Navn,arrangement = delegat.Arrangement.Navn, pin = delegat.Delegatkode, url = baseUri });
            delegat.SendtEmail = DateTimeOffset.Now;
            await context.SaveChangesAsync();
        }

        public async Task<bool> SendSms(int delegatId, string baseUri)
        {
            await using var context = _contextFactory.CreateDbContext();
            var delegat = await context.Delegat
                .Where(d => d.Id == delegatId)
                .FirstOrDefaultAsync();
            if (delegat == null)
                throw new StemmeException($"Fant ikke delegat med id{delegatId}");
            if (string.IsNullOrEmpty(delegat.Telefon))
                throw new StemmeException($"Delegat {delegat.Delegatnummer} ({delegat.Navn}) har ikke telefonnummer");
            
            var msg = $"Hei. {delegat.Navn} har PIN-kode {delegat.Delegatkode} i Romsdal og Nordmøre krets sitt system for elektronisk avstemning. Bruk PIN-koden på {baseUri} for å avgi din stemme. Du kan også bruke lenken: {baseUri}pin/{delegat.Delegatkode}";

            var success = await _smsSender.SendSms(delegat.Telefon, msg);
            if (success)
            {
                delegat.SendtSms = DateTimeOffset.Now;
                await context.SaveChangesAsync();
            }
            return success;
        }
    }
}