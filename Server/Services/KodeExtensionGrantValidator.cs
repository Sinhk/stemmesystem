using System.Security.Claims;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Validation;
using IdentityModel;
using Stemmesystem.Data.Repositories;
using Stemmesystem.Shared;

namespace Stemmesystem.Server.Services;

public class KodeExtensionGrantValidator : IExtensionGrantValidator
{
    private readonly IDelegateRepository _delegateRepository;

    public KodeExtensionGrantValidator(IDelegateRepository delegateRepository)
    {
        _delegateRepository = delegateRepository;
    }
    
    public async Task ValidateAsync(ExtensionGrantValidationContext context)
    {
        var delegateCode = context.Request.Raw["delegatkode"];
        if (string.IsNullOrEmpty(delegateCode))
        {
            context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "ingen delegatkode funnet");
            return;
        }

        var delegateEntity = await _delegateRepository.ValidateCode(delegateCode);
        if (delegateEntity == null)
        {
            context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "ugyldig delegatkode");
            return;
        }

        var claims = new List<Claim>
        {
            new(JwtClaimTypes.Role, "Delegat"),
            new(AuthConstants.ArrangementClaimType, delegateEntity.ArrangementId.ToString()),
        };
        if (delegateEntity.Name != null) 
            claims.Add(new Claim(JwtClaimTypes.Name, delegateEntity.Name));

        if (delegateEntity.Email != null)
            claims.Add(new Claim(JwtClaimTypes.Email, delegateEntity.Email));
        
        if (delegateEntity.Phone != null)
            claims.Add(new Claim(JwtClaimTypes.PhoneNumber, delegateEntity.Phone));

        context.Result = new GrantValidationResult(delegateEntity.DelegateCode, AuthConstants.DelegatkodeGrantType, claims);
    }

    public string GrantType => AuthConstants.DelegatkodeGrantType;
}