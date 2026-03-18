using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using Stemmesystem.Api;
using Xunit;

namespace Stemmesystem.Server.Tests;

public class AutoMapperTests
{
    [Fact]
    public void ConfigurationIsValid()
    {
        var configuration = new MapperConfiguration(c => c.AddProfile(typeof(ApiAutoMapperProfile)), NullLoggerFactory.Instance);
        var mapper = configuration.CreateMapper();
        
        mapper.ConfigurationProvider.AssertConfigurationIsValid();
    }
}