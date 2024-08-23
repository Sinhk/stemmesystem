namespace Stemmesystem.Server.Auth;

public static class ClaimsPrincipalExtensions
{
    public static string GetDelegatkode(this System.Security.Claims.ClaimsPrincipal user)
    {
        return user.FindFirst("sub")?.Value!;
    }
}