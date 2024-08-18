using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Stemmesystem.Server.Tests.Integration;

public class BasicTests : IClassFixture<ServerFactory>
{
    private readonly ServerFactory _factory;

    public BasicTests(ServerFactory factory)
    {
        _factory = factory;
    }

    [Theory]
    [InlineData("/healthz", "text/plain")]
    public async Task Get_EndpointsReturnSuccessAndCorrectContentType(string url, string contentType)
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync(url);

        // Assert
        response.Should().BeSuccessful();
        response.Content.Headers.ContentType?.ToString().Should().Be(contentType);
    }
}