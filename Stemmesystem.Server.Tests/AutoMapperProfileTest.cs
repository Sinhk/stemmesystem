using AutoMapper;
using Stemmesystem.Api;
using Xunit;

namespace Stemmesystem.Server.Tests;

public class AutoMapperTests
{
    [Fact]
    public void ConfigurationIsValid()
    {
        var configuration = new MapperConfiguration(c => c.AddProfile(typeof(ApiAutoMapperProfile)));
        var mapper = configuration.CreateMapper();
        
        mapper.ConfigurationProvider.AssertConfigurationIsValid();
    }
}