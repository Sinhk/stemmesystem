using System.Security.Claims;
using System.Security.Principal;
using Duende.IdentityModel;

namespace Stemmesystem.Server;

public static class PrincipalExtensions
{
    public static string GetSubjectId(this IPrincipal principal) => principal.Identity.GetSubjectId();
    
    public static string GetSubjectId(this IIdentity? identity)
    {
        var id = identity as ClaimsIdentity;
        var claim = id?.FindFirst(JwtClaimTypes.Subject);

        if (claim == null) throw new InvalidOperationException("sub claim is missing");
        return claim.Value;
    }

    public static bool IsAuthenticated(this IPrincipal? principal) => principal is { Identity.IsAuthenticated: true };
}