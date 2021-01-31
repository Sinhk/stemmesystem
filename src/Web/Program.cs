using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Stemmesystem.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

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
                
                if (!db.Users.Any())
                {
                    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
                    await CreateAdminUsers(userManager);
                }
            }

            await host.RunAsync();
        }

        private static async Task CreateAdminUsers(UserManager<IdentityUser> userManager)
        {
            await userManager.CreateAsync(new IdentityUser("sindre.kroknes@gmail.com") {Email = " sindre.kroknes@gmail.com", EmailConfirmed = true});
            await userManager.CreateAsync(new IdentityUser("patrickg@romnorkrets.no") {Email = " patrickg@romnorkrets.no", EmailConfirmed = true});
            await userManager.CreateAsync(new IdentityUser("siljeth.kroknes@gmail.com") {Email = " siljeth.kroknes@gmail.com", EmailConfirmed = true});
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
