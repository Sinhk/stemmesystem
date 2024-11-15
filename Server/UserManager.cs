using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Stemmesystem.Data;
using Stemmesystem.Data.Entities;
using Stemmesystem.Data.Models;
using Stemmesystem.Server.Data.Entities;
using Stemmesystem.Server.Services;
using Stemmesystem.Shared.Models;

namespace Stemmesystem.Server;


public  class UserManager
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UserManager(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager )
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }
    private const string AdminRoleName = "admin";

    internal static async Task SeedData(StemmesystemContext db, DelegatService delegatService)
    {
        await using var transaction =  await db.Database.BeginTransactionAsync();
        Arrangement arrangement = new("Testarrangement") { Beskrivelse = "Bare en test" };
        Sak sak = new(1, "Testsak 1") { Beskrivelse = "Sak for å teste stemmesystemet" };
        var votering1 = new Votering("Skal vi ha kretsting?", false, "Ja", "Nai", "Kanskje");
        var votering2 = new Votering("Beste farge", true, 2, new[] { "Rød", "Gul", "Grønn", "Blå" });
        var votering3 = new Votering("Valg av person", false, 2, "Patrick","Elin","Torbjørn","Annette", "Kjetil", "May Britt", "Odd Kjetil", "Ole", "Silje", "Synnøve","Åge");
        sak.LeggTil(votering1, votering2, votering3);
        arrangement.LeggTil(sak);

        db.Arrangement.Add(arrangement);
        await db.SaveChangesAsync();
        
        await delegatService.RegistrerNyDelegat(new DelegatInputModel { ArrangementId = arrangement.Id, Delegatnummer = 1, Navn = "Sindre", Epost = "sindre.kroknes@gmail.com", Telefon = "99150713" });
        await delegatService.RegistrerNyDelegat(new DelegatInputModel { ArrangementId = arrangement.Id, Delegatnummer = 2, Navn = "Silje", Epost = "siljeth.kroknes@gmail.com"});
        await delegatService.RegistrerNyDelegat(new DelegatInputModel { ArrangementId = arrangement.Id, Delegatnummer = 3, Navn = "Patrick", Epost = "patrick.gule@gmail.com" });

        await db.SaveChangesAsync();

        await transaction.CommitAsync();

    }

    public async Task AddMissingAdminUsers()
    {
        if (!await _roleManager.RoleExistsAsync(AdminRoleName)) {
            await _roleManager.CreateAsync(new IdentityRole(AdminRoleName));
        }

        var adminUsers = new HashSet<string>
        {
            "sindre.kroknes@speiding.no",
            "tom.lantz@speiding.no",
            "birgit.helene.torsaeter@speiding.no",
            "lise.ringstad@speiding.no",
            "andreas.salater@speiding.no",
            "ingeborg.kolstad@speiding.no",
            "kirsten.eidem@speiding.no","oda.larsen@speiding.no", "katinka.rosbach@speiding.no", "brede.udahl@speiding.no", "tove.jurs-martinsen@speiding.no", "eva.brunsell@speiding.no", "ingrid.stene.kvist@speiding.no", "birgit.helene.torsaeter@speiding.no"
        };

        var existing = await _userManager.Users
            .Select(u => u.Email)
            .ToArrayAsync();

        var toAdd = adminUsers.Except(existing);

        foreach (var username in toAdd)
        {
            await CreateUser(username);
        }
    }

    private async Task CreateUser(string email)
    {
        var user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
        await _userManager.CreateAsync(user);
        await _userManager.AddToRoleAsync(user, AdminRoleName);
    }
}