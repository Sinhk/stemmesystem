using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using StemmeSystem.Data;
using Stemmesystem.Server.Data;
using Stemmesystem.Server.InternalServices;
using Stemmesystem.Shared;
using Stemmesystem.Shared.Interfaces;

namespace Stemmesystem.Server.Services;

[Authorize(Roles = "admin")]
public class PinSender : IPinSender
{
    private readonly ISmsSender _smsSender;
    private readonly IEpostSender _emailSender;
    private readonly StemmesystemContext _context;

    public PinSender(ISmsSender smsSender, IEpostSender emailSender, StemmesystemContext context)
    {
        _smsSender = smsSender;
        _emailSender = emailSender;
        _context = context;
    }

    public async Task SendEmail(SendPinRequest request)
    {
        var (delegatId, baseUrl) = request;
        var delegat = await _context.Delegat
            .Where(d => d.Id == delegatId)
            .Include(d=> d.Arrangement)
            .FirstOrDefaultAsync();
        if (delegat == null)
            throw new StemmeException($"Fant ikke delegat med id{delegatId}");
        if (string.IsNullOrEmpty(delegat.Epost))
            throw new StemmeException($"Delegat {delegat.Delegatnummer} ({delegat.Navn}) har ikke e-postadresse");
            
        //var msg = $"Hei. {delegat.Navn} har PIN-kode {delegat.Delegatkode} i Romsdal og Nordmøre krets sitt system for elektronisk avstemning. Bruk PIN-koden på {baseUri} for å avgi din stemme i de sakene der du har stemmerett. Du kan også bruke lenken: {baseUri}/pin/{delegat.Delegatkode}";
        //await _emailSender.SendEmailAsync(delegat.Epost,$"PIN-kode til {delegat.Arrangement.Navn}",msg);
        await _emailSender.SendEmailTemplateAsync(delegat.Epost, delegat.Navn, "d-c4f5f9ba460546779394bce9d3ab97b8", new {navn = delegat.Navn,arrangement = delegat.Arrangement.Navn, pin = delegat.Delegatkode, url = baseUrl });
        delegat.SendtEmailInternal = DateTimeOffset.Now;
        await _context.SaveChangesAsync();
    }

    public async Task SendSms(SendPinRequest request)
    {
        var (delegatId, baseUrl) = request;
        var delegat = await _context.Delegat
            .Where(d => d.Id == delegatId)
            .FirstOrDefaultAsync();
        if (delegat == null)
            throw new StemmeException($"Fant ikke delegat med id{delegatId}");
        if (string.IsNullOrEmpty(delegat.Telefon))
            throw new StemmeException($"Delegat {delegat.Delegatnummer} ({delegat.Navn}) har ikke telefonnummer");
            
        var msg = $"Hei. {delegat.Navn} har PIN-kode {delegat.Delegatkode} i Romsdal og Nordmøre krets sitt system for elektronisk avstemning. Bruk PIN-koden på {baseUrl} for å avgi din stemme. Du kan også bruke lenken: {delegat}pin/{delegat.Delegatkode}";

        var success = await _smsSender.SendSms(delegat.Telefon, msg);
        if (success)
        {
            delegat.SendtSmsInternal = DateTimeOffset.Now;
            await _context.SaveChangesAsync();
        }
    }
}