using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Stemmesystem.Data;
using Stemmesystem.Tools;
using Stemmesystem.Web.Data;
using System.Threading.Tasks;

namespace Stemmesystem.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            IHost host = CreateHostBuilder(args).Build();
            var environment = host.Services.GetRequiredService<IHostEnvironment>();

            using (var scope = host.Services.CreateScope())
            {
                StemmesystemContext db = scope.ServiceProvider.GetRequiredService<StemmesystemContext>();
                var delgatService = scope.ServiceProvider.GetRequiredService<IDelegatService>();

                if (environment.IsDevelopment())
                {
                    db.Database.EnsureDeleted();
                    db.Database.EnsureCreated();

                    Arrangement arrangement = new Arrangement("Testarrangement") { Beskrivelse = "Bare en test" };
                    Sak sak = new Sak(1, "Testsak 1") { Beskrivelse = "Sak for å teste stemmesystemet" };
                    var votering1 = new EnkelVotering("Skal vi ha kretsting?", false, "Ja", "Nai", "Kanskje");
                    var votering2 = new Flervalgsvotering("Hvem er best?", new[] { "Per", "Pål", "Espen Askeladd" });
                    sak.LeggTil(votering1, votering2);
                    arrangement.LeggTil(sak);
                    delgatService.RegistrerNyDelegat(arrangement, new(1) {Navn = "Sindre", Epost = "sindre.kroknes@gmail.com", Telefon = "99150713" });
                    delgatService.RegistrerNyDelegat(arrangement, new(2) {Navn = "Silje" });
                    delgatService.RegistrerNyDelegat(arrangement, new(3) {Navn = "Patrick" });

                    db.Arrangement.Add(arrangement);

                    db.SaveChanges();
                }
                else
                {
                    db.Database.Migrate();
                }
            }

            using (var scope = host.Services.CreateScope())
            {
                var stemmeService = scope.ServiceProvider.GetRequiredService<StemmeService>();
                var aktiv = await stemmeService.AktivVotering(1);
                if (aktiv == null)
                    await stemmeService.StartVotering(1, 1);
            }

            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
