using Microsoft.AspNetCore.Mvc;

namespace Stemmesystem.Server.Controllers;

public class OidcConfigurationController : Controller
{
    private const string DefaultScope = "openid profile Stemmesystem.ServerAPI";

    private readonly ILogger<OidcConfigurationController> _logger;

    public OidcConfigurationController(ILogger<OidcConfigurationController> logger)
    {
        _logger = logger;
    }

    [HttpGet("_configuration/{clientId}")]
    public IActionResult GetClientRequestParameters([FromRoute] string clientId)
    {
        var request = HttpContext.Request;
        var baseUrl = $"{request.Scheme}://{request.Host}";

        return Ok(new Dictionary<string, string>
        {
            { "authority", baseUrl },
            { "client_id", clientId },
            { "redirect_uri", $"{baseUrl}/authentication/login-callback" },
            { "post_logout_redirect_uri", $"{baseUrl}/authentication/logout-callback" },
            { "response_type", "code" },
            { "scope", DefaultScope },
            { "defaultScopes", DefaultScope },
        });
    }
}

