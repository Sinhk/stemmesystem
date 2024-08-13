using Microsoft.EntityFrameworkCore;
using StemmeSystem.Data;
using Stemmesystem.Server.Services;

namespace Stemmesystem.Server;

internal static class ServiceConfiguration
{
    public static void MapGrpcServices(this WebApplication webApplication)
    {
        webApplication.MapGrpcService<DelegatService>();
        webApplication.MapGrpcService<SakService>();
        webApplication.MapGrpcService<ArrangementService>();
        webApplication.MapGrpcService<StemmeService>();
        webApplication.MapGrpcService<PinSender>();
    }
    
    public static void AddAppDbContext(this IServiceCollection services)
    {
        services.AddDbContext<StemmesystemContext>((provider, builder) =>
        {
            var configuration = provider.GetRequiredService<IConfiguration>();
            var dbProvider = configuration.GetValue("Provider", "Postgres");
            switch (dbProvider)
            {
                case "Sqlite":
                    builder.UseSqlite(configuration.GetConnectionString("DefaultConnection"),
                        x => x.MigrationsAssembly("SqliteMigrations"));
                    break;
                case "SqlServer":
                    builder.UseSqlServer(configuration.GetConnectionString("SqlServerConnection") ?? "not-provided",
                        x => x.MigrationsAssembly("SqlServerMigrations"));
                    break;
                case "Postgres":
                    builder.UseNpgsql(configuration.GetConnectionString("PostgresConnection") ?? "not-provided",
                        x => x.MigrationsAssembly("PostgresMigrations"));
                    break;
                default:
                    throw new Exception($"Unsupported provider: {dbProvider}");
            }
            
        });
    }
}