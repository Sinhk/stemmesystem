using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;

namespace Stemmesystem.Client.Tests;

public class AutoMapperTests
{
    [Fact]
    public void ConfigurationIsValid()
    {
        var configuration = new MapperConfiguration(c => c.AddProfile(typeof(WebAutoMapperProfile)), NullLoggerFactory.Instance);
        var mapper = configuration.CreateMapper();
        
        mapper.ConfigurationProvider.AssertConfigurationIsValid();
    }
}