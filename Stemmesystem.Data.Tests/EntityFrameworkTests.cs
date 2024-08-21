using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Stemmesystem.Data;

namespace Stemmesystem.Data.Tests;

public class EntityFrameworkTests
{
    [Fact]
    public void DbContext_ShouldHaveNoPendingChanges()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<StemmesystemContext>()
            .UseNpgsql()
            .Options;
        var sut = new StemmesystemContext(options);
        
        // Act & Assert
        sut.Database.HasPendingModelChanges().Should().BeFalse();
    }
}