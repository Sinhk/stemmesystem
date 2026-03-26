using Microsoft.EntityFrameworkCore;
using Stemmesystem.Data;
using Stemmesystem.Server.Features.MinSpeiding;
using Stemmesystem.Server.Services;

namespace Stemmesystem.Server;

internal static class ServiceConfiguration
{
    public static void MapGrpcServices(this WebApplication webApplication)
    {
        webApplication.MapGrpcService<DelegateService>();
        webApplication.MapGrpcService<CaseService>();
        webApplication.MapGrpcService<ArrangementService>();
        webApplication.MapGrpcService<VoteService>();
        webApplication.MapGrpcService<PinSender>();
        webApplication.MapGrpcService<MinSpeidingOptionsRepository>();
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