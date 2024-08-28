using System.Security.Claims;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Validation;
using IdentityModel;
using Stemmesystem.Data.Repositories;
using Stemmesystem.Shared;

namespace Stemmesystem.Server.Services;

public class KodeExtensionGrantValidator : IExtensionGrantValidator
{
    private readonly IDelegatRepository _delegatRepository;

    public KodeExtensionGrantValidator(IDelegatRepository delegatRepository)
    {
        _delegatRepository = delegatRepository;
    }
    
    public async Task ValidateAsync(ExtensionGrantValidationContext context)
    {
        var delegatkode = context.Request.Raw["delegatkode"];
        if (string.IsNullOrEmpty(delegatkode))
        {
            context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "ingen delegatkode funnet");
            return;
        }

        var delegat = await _delegatRepository.ValiderKode(delegatkode);
        if (delegat == null)
        {
            context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "ugyldig delegatkode");
            return;
        }

        var claims = new List<Claim>
        {
            new(JwtClaimTypes.Role, "Delegat"),
            new(AuthConstants.ArrangementClaimType, delegat.ArrangementId.ToString()),
        };
        if (delegat.Navn != null) 
            claims.Add(new Claim(JwtClaimTypes.Name, delegat.Navn));

        if (delegat.Epost != null)
            claims.Add(new Claim(JwtClaimTypes.Email, delegat.Epost));
        
        if (delegat.Telefon != null)
            claims.Add(new Claim(JwtClaimTypes.PhoneNumber, delegat.Telefon));

        context.Result = new GrantValidationResult(delegat.Delegatkode, AuthConstants.DelegatkodeGrantType, claims);
    }

    public string GrantType => AuthConstants.DelegatkodeGrantType;
}