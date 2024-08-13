using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using StemmeSystem.Data;
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
    private readonly ILogger<PinSender> _logger;

    public PinSender(ISmsSender smsSender, IEpostSender emailSender, StemmesystemContext context, ILogger<PinSender> logger)
    {
        _smsSender = smsSender;
        _emailSender = emailSender;
        _context = context;
        _logger = logger;
    }

    public async Task<SendPinResult> SendEmail(SendPinRequest request, CancellationToken cancellationToken = default)
    {
        var (delegatId, baseUrl) = request;
        var delegat = await _context.Delegat
            .Where(d => d.Id == delegatId)
            .Include(d=> d.Arrangement)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        if (delegat == null)
            throw new StemmeException($"Fant ikke delegat med id {delegatId}");
        if (string.IsNullOrEmpty(delegat.Epost))
            throw new StemmeException($"Delegat {delegat.Delegatnummer} ({delegat.Navn}) har ikke e-postadresse");
            
        var msg = $"Hei {delegat.Navn}. Din PIN-kode for avstemming på {delegat.Arrangement.Navn} er: {delegat.Delegatkode}. Bruk PIN-koden på {baseUrl} for å avgi din stemme i de sakene der du har stemmerett. Du kan også bruke lenken: {baseUrl}/pin/{delegat.Delegatkode}";
        var model = new EpostModel(delegat.Epost, $"PIN-kode til {delegat.Arrangement.Navn}")
        {
            TilNavn = delegat.Navn,
            PlainTextMessage = msg
        };
        
        try
        {
            await _emailSender.SendEmailAsync(model);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Feil ved sending av e-post til {Delegat}", delegat.Navn);
            return new SendPinResult(false);
        }

        delegat.SendtEmailInternal = DateTimeOffset.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return new SendPinResult(true);
    }

    public async Task<SendPinResult> SendSms(SendPinRequest request, CancellationToken cancellationToken = default)
    {
        var (delegatId, baseUrl) = request;
        var delegat = await _context.Delegat
            .Where(d => d.Id == delegatId)
            .Include(d => d.Arrangement)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        if (delegat == null)
            throw new StemmeException($"Fant ikke delegat med id{delegatId}");
        if (string.IsNullOrEmpty(delegat.Telefon))
            throw new StemmeException($"Delegat {delegat.Delegatnummer} ({delegat.Navn}) har ikke telefonnummer");

        var uri = new Uri(baseUrl, UriKind.Absolute);
        var msg = $"Hei {delegat.Navn}. Din PIN-kode for avstemming på {delegat.Arrangement.Navn} er {delegat.Delegatkode}. Bruk PIN-koden på {uri} for å avgi din stemme. Du kan også bruke lenken: {new Uri(uri,$"pin/{delegat.Delegatkode}")}";

        var success = await _smsSender.SendSms(delegat.Telefon, msg);
        if (success)
        {
            delegat.SendtSmsInternal = DateTimeOffset.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
        
        return new SendPinResult(success);
    }
}