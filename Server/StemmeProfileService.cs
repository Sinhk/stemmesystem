using Duende.IdentityServer.AspNetIdentity;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Models;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Stemmesystem.Core;
using StemmeSystem.Data.Models;

namespace Stemmesystem.Server;

public sealed class StemmeProfileService : ProfileService<ApplicationUser>
{
    public override async Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        if (context.Subject.GetAuthenticationMethod() == AuthConstants.DelegatkodeGrantType)
        {
            //TODO: Find out how to populate requested claims properly
            context.RequestedClaimTypes = context.RequestedClaimTypes
                .Append(AuthConstants.ArrangementClaimType)
                .Append(JwtClaimTypes.Name)
                .Append(JwtClaimTypes.Email)
                .Append(JwtClaimTypes.Role)
                .Append(JwtClaimTypes.PhoneNumber);
            context.AddRequestedClaims(context.Subject.Claims);
            return;
        }
        await base.GetProfileDataAsync(context);
    }

    public override async Task IsActiveAsync(IsActiveContext context)
    {
        if (context.Subject.GetAuthenticationMethod() == AuthConstants.DelegatkodeGrantType)
        {
            context.IsActive = true;
            return;
        }

        await base.IsActiveAsync(context);
    }

    public StemmeProfileService(UserManager<ApplicationUser> userManager, IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory) : base(userManager, claimsFactory)
    {
    }

    public StemmeProfileService(UserManager<ApplicationUser> userManager, IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory, ILogger<ProfileService<ApplicationUser>> logger) : base(userManager, claimsFactory, logger)
    {
    }
}