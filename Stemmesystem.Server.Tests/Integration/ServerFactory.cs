using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        
        builder.UseEnvironment("Development");
    }

    /// <inheritdoc />
    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
    }

    /// <inheritdoc />
    async Task IAsyncLifetime.DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
    }
}