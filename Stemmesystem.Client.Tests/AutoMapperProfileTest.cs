using AutoMapper;

namespace Stemmesystem.Client.Tests;

public class AutoMapperTests
{
    [Fact]
    public void ConfigurationIsValid()
    {
        var configuration = new MapperConfiguration(c => c.AddProfile(typeof(WebAutoMapperProfile)));
        var mapper = configuration.CreateMapper();
        
        mapper.ConfigurationProvider.AssertConfigurationIsValid();
    }
}