using Duende.IdentityServer.Validation;
using DuendeClient = Duende.IdentityServer.Models.Client;

namespace Stemmesystem.Server.Auth;

/// <summary>
/// Validates redirect URIs by allowing any URI that originates from the same host as the server.
/// This replaces the RelativeRedirectUriValidator from Microsoft.AspNetCore.ApiAuthorization.IdentityServer
/// which is incompatible with Duende.IdentityServer 7.x.
/// </summary>
public class SameOriginRedirectUriValidator : IRedirectUriValidator
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SameOriginRedirectUriValidator(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<bool> IsRedirectUriValidAsync(string requestedUri, DuendeClient client)
        => Task.FromResult(IsSameOrigin(requestedUri) || IsInClientList(requestedUri, client.RedirectUris));

    public Task<bool> IsPostLogoutRedirectUriValidAsync(string requestedUri, DuendeClient client)
        => Task.FromResult(IsSameOrigin(requestedUri) || IsInClientList(requestedUri, client.PostLogoutRedirectUris));

    private bool IsSameOrigin(string requestedUri)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context is null)
        {
            // During startup validation or background tasks there is no HTTP context;
            // fall through to the explicit client-list check.
            return false;
        }

        if (!Uri.TryCreate(requestedUri, UriKind.Absolute, out var uri)) return false;

        return string.Equals(
            $"{uri.Scheme}://{uri.Authority}",
            $"{context.Request.Scheme}://{context.Request.Host}",
            StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsInClientList(string requestedUri, ICollection<string>? allowedUris)
        => allowedUris?.Contains(requestedUri, StringComparer.OrdinalIgnoreCase) == true;
}
