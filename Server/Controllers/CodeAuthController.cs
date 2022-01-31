using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StemmeSystem.Data.Models;

namespace Stemmesystem.Server.Controllers;

[Route("api/kode")]
public class CodeAuthController : ControllerBase
{
    private readonly SignInManager<ApplicationUser> _signInManager;

    public CodeAuthController(SignInManager<ApplicationUser> signInManager)
    {
        _signInManager = signInManager;
    }

    [HttpPost("{kode}")]
    public async Task<IActionResult> Login(string kode)
    {
        if (kode != "ABXY3")
            return BadRequest("Ukjent kode");

        var user = new ApplicationUser
        {
            Email = "sindre.kroknes@gmail.com",
            Id = kode,
            EmailConfirmed = true,
            UserName = "Sindre",
            PhoneNumber = "99150713"
        };
        AuthenticationProperties properties = new AuthenticationProperties()
        {
            
        };
        await _signInManager.SignInWithClaimsAsync(user, properties, new[] { new Claim(ClaimTypes.Role, "Delegat") });

        return Ok();
    }
}