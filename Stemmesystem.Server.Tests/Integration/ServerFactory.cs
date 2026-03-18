using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Testcontainers.PostgreSql;
using Xunit;

namespace Stemmesystem.Server.Tests.Integration;

public class ServerFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder().Build();

    /// <inheritdoc />
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        IEnumerable<KeyValuePair<string,string>> data = new Dictionary<string, string>
        {
            {"ConnectionStrings:PostgresConnection", _dbContainer.GetConnectionString()}
        };

        builder.ConfigureAppConfiguration(c => c.AddInMemoryCollection(data));

        builder.ConfigureServices(services =>
        {
            RemoveGoogleAuth(services);

            services.AddAuthentication(defaultScheme: "TestScheme")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                        "TestScheme", options => { });
        });
        
        builder.UseEnvironment("Development");
    }

    private static void RemoveGoogleAuth(IServiceCollection services)
    {
        services.RemoveAll<GoogleHandler>();
        services.RemoveAll<IConfigureOptions<GoogleOptions>>();
        services.RemoveAll<IValidateOptions<GoogleOptions>>();
    }

    /// <inheritdoc />
    public async ValueTask InitializeAsync()
    {
        await _dbContainer.StartAsync();
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
    }
}