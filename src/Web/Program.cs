using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Stemmesystem.Data;
using Stemmesystem.Web.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Stemmesystem.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            IHost host = CreateHostBuilder(args).Build();
            //var environment = host.Services.GetRequiredService<IHostEnvironment>();

            using (var scope = host.Services.CreateScope())
            {
                IDbContextFactory<StemmesystemContext> contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<StemmesystemContext>>();
                await using var db = contextFactory.CreateDbContext();
                
                await db.Database.MigrateAsync();
                
                if (!db.Arrangement.Any())
                {
                    var delegatService = scope.ServiceProvider.GetRequiredService<IDelegatService>();
                    await SeedData(db, delegatService);
                }
            }

            await host.RunAsync();
        }

        private static async Task SeedData(StemmesystemContext db, IDelegatService delegatService)
        {
            Arrangement arrangemento = new("Testkretsting") { Beskrivelse = "Ikke bare-bare en test", Logo = "romnorkrets.png" };
	    Sak saka = new(1, "Testsak 1") { Beskrivelse = "Sak for å teste stemmesystemet" };
            var votering4 = new Votering("Skal vi ha kretsting igjen?", false, "Ja", "Nai", "Kanskje");
            saka.LeggTil(votering4);
            arrangemento.LeggTil(saka);
            Sak sake = new(5, "Testsak 5") { Beskrivelse = "Enda flere saker" };
            var votering5 = new Votering("Beste farge på kretsting", true, 2, new[] { "Rød", "Gul", "Grønn", "Blå" });
            sake.LeggTil(votering5);
            arrangemento.LeggTil(sake);
            delegatService.RegistrerNyDelegat(arrangemento, new(10) { Navn = "SindreHK", Epost = "sindre.kroknes@gmail.com", Telefon = "99150713" });
            delegatService.RegistrerNyDelegat(arrangemento, new(2) { Navn = "SiljeTHK", Epost = "siljeth.kroknes@gmail.com"});
            delegatService.RegistrerNyDelegat(arrangemento, new(1) { Navn = "PatrickG", Epost = "patrick.gule@gmail.com" });

            db.Arrangement.Add(arrangemento);

            Arrangement arrangement = new("Testarrangement") { Beskrivelse = "Bare en test", Logo = "testkrets.png" };
            Sak sak = new(1, "Testsak 1") { Beskrivelse = "Sak for å teste stemmesystemet" };
            var votering1 = new Votering("Skal vi ha kretsting?", false, "Ja", "Nai", "Kanskje");
            var votering2 = new Votering("Beste farge", true, 2, new[] { "Rød", "Gul", "Grønn", "Blå" });
            var votering3 = new Votering("Valg av person", false, 2, "Patrick","Elin","Torbjørn","Annette", "Kjetil", "May Britt", "Odd Kjetil", "Ole", "Silje", "Synnøve","Åge");
            sak.LeggTil(votering1, votering2, votering3);
            arrangement.LeggTil(sak);
            delegatService.RegistrerNyDelegat(arrangement, new(1) { Navn = "Sindre", Epost = "sindre.kroknes@gmail.com", Telefon = "99150713" });
            delegatService.RegistrerNyDelegat(arrangement, new(2) { Navn = "Silje", Epost = "siljeth.kroknes@gmail.com"});
            delegatService.RegistrerNyDelegat(arrangement, new(3) { Navn = "Patrick", Epost = "patrick.gule@gmail.com" });

            db.Arrangement.Add(arrangement);

            await db.SaveChangesAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    var port = Environment.GetEnvironmentVariable("PORT");
                    if (!string.IsNullOrEmpty(port))
                        webBuilder.UseUrls($"http://*:{port}");
                    webBuilder.UseStartup<Startup>();
                });
    }
}
