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
            builder.UseNpgsql(configuration.GetConnectionString("PostgresConnection"));
        });
    }
}