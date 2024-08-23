using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Stemmesystem.Core.Tools;
using Stemmesystem.Data;
using Stemmesystem.Data.Models;
using Stemmesystem.Data.Repositories;
using Stemmesystem.Server.InternalServices;
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
        
        services.AddDatabaseDeveloperPageExceptionFilter();
    }
    
    public static void AddAuthServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDataProtection()
            .PersistKeysToDbContext<StemmesystemContext>();

        var authenticationBuilder = services.AddAuthentication(IdentityConstants.ApplicationScheme);
        authenticationBuilder.AddIdentityCookies();

        var googleAuth = configuration
            .GetSection("Authentication:Google");
        if (googleAuth.Exists())
        {
            authenticationBuilder.AddGoogle(options =>
            {
                options.ClientId = googleAuth["ClientId"]!;
                options.ClientSecret = googleAuth["ClientSecret"]!;
            });
        }

        services.AddAuthorizationBuilder();

        services.AddIdentityCore<StemmeUser>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<StemmesystemContext>()
            .AddApiEndpoints();
    }
    
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IDelegatRepository, DelegatRepository>();
        services.AddScoped<IArrangementRepository, ArrangementRepository>();
        services.AddScoped<DelegatService>();
        services.AddScoped<NotificationManager>();

        services.AddHttpClient<ISmsSender, SveveSmsSender>();
        services.AddOptions<SveveOptions>()
            .BindConfiguration("Sveve")
            .ValidateDataAnnotations();

        services.AddTransient<IEmailSender, EmailSender>();
        services.AddTransient<IEpostSender, EmailSender>();
        services.AddOptions<EmailSettings>()
            .BindConfiguration("EmailSettings")
            .ValidateDataAnnotations();

        services.AddSingleton<IKeyGenerator, RngKeyGenerator>();
        services.AddSingleton<IKeyHasher, KeyHasher>();

        return services;
    }

}