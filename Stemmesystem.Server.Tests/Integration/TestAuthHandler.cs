using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Stemmesystem.Server.Tests.Integration;

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    /// <inheritdoc />
    public TestAuthHandler(
        [NotNull] IOptionsMonitor<AuthenticationSchemeOptions> options, 
        [NotNull] ILoggerFactory logger, 
        [NotNull] UrlEncoder encoder) 
        : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[] { new Claim(ClaimTypes.Name, "Test user") };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "TestScheme");

        var result = AuthenticateResult.Success(ticket);

        return Task.FromResult(result);
    }
}