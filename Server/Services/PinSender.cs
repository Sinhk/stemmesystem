using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Stemmesystem.Data;
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
        var (delegateId, baseUrl) = request;
        var delegateEntity = await _context.Delegates
            .Where(d => d.Id == delegateId)
            .Include(d=> d.Arrangement)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        if (delegateEntity == null)
            throw new VotingException($"Fant ikke delegat med id {delegateId}");
        if (string.IsNullOrEmpty(delegateEntity.Email))
            throw new VotingException($"Delegat {delegateEntity.DelegateNumber} ({delegateEntity.Name}) har ikke e-postadresse");
            
        var msg = $"Hei {delegateEntity.Name}. Din PIN-kode for avstemming på {delegateEntity.Arrangement.Name} er: {delegateEntity.DelegateCode}. Bruk PIN-koden på {baseUrl} for å avgi din stemme i de sakene der du har stemmerett. Du kan også bruke lenken: {baseUrl}/pin/{delegateEntity.DelegateCode}";
        var model = new EpostModel(delegateEntity.Email, $"PIN-kode til {delegateEntity.Arrangement.Name}")
        {
            TilNavn = delegateEntity.Name,
            PlainTextMessage = msg
        };
        
        try
        {
            await _emailSender.SendEmailAsync(model);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Feil ved sending av e-post til {Delegat}", delegateEntity.Name);
            return new SendPinResult(false);
        }

        delegateEntity.EmailSentAtInternal = DateTimeOffset.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return new SendPinResult(true);
    }

    public async Task<SendPinResult> SendSms(SendPinRequest request, CancellationToken cancellationToken = default)
    {
        var (delegateId, baseUrl) = request;
        var delegateEntity = await _context.Delegates
            .Where(d => d.Id == delegateId)
            .Include(d => d.Arrangement)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        if (delegateEntity == null)
            throw new VotingException($"Fant ikke delegat med id{delegateId}");
        if (string.IsNullOrEmpty(delegateEntity.Phone))
            throw new VotingException($"Delegat {delegateEntity.DelegateNumber} ({delegateEntity.Name}) har ikke telefonnummer");

        var uri = new Uri(baseUrl, UriKind.Absolute);
        var msg = $"Hei {delegateEntity.Name}. Din PIN-kode for avstemming på {delegateEntity.Arrangement.Name} er {delegateEntity.DelegateCode}. Bruk PIN-koden på {uri} for å avgi din stemme. Du kan også bruke lenken: {new Uri(uri,$"pin/{delegateEntity.DelegateCode}")}";

        var success = await _smsSender.SendSms(delegateEntity.Phone, msg);
        if (success)
        {
            delegateEntity.SmsSentAtInternal = DateTimeOffset.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
        
        return new SendPinResult(success);
    }
}