using Microsoft.AspNetCore.Identity;
using Stemmesystem.Data;
using Stemmesystem.Data.Entities;
using Stemmesystem.Data.Models;
using Stemmesystem.Server.Data.Entities;
using Stemmesystem.Server.Services;
using Stemmesystem.Shared.Models;

namespace Stemmesystem.Server;

public static class TestData
{
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

    public static async Task CreateAdminUsers(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        if (!await roleManager.RoleExistsAsync(AdminRoleName)) {
            await roleManager.CreateAsync(new IdentityRole(AdminRoleName));
        }

        await CreateUser(userManager, "sindre.kroknes@speiding.no");
        await CreateUser(userManager, "tom.lantz@speiding.no");
    }

    internal static async Task<ApplicationUser> CreateUser(UserManager<ApplicationUser> userManager, string email)
    {
        var user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
        await userManager.CreateAsync(user);
        await userManager.AddToRoleAsync(user, AdminRoleName);
        return user;
    }
}