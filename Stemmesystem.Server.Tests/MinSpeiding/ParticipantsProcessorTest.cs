using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using FluentAssertions;
using JetBrains.Annotations;
using Stemmesystem.Server.Features.MinSpeiding;
using Xunit;

namespace Stemmesystem.Server.Tests.MinSpeiding;

[TestSubject(typeof(ParticipantsProcessor))]
public partial class ParticipantsProcessorTest
{
  [Fact]
    public void ProcessParticipants_ShouldReturnParsedParticipants()
    {
        // Arrange
        var data = JsonNode.Parse(ParticipantsData)!;
        var participantCount = data.AsObject()["participants"].Deserialize<IReadOnlyDictionary<string,object>>()!.Count;
        
        // Act
        var result = ParticipantsProcessor.ParseParticipants(data);
        
        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(participantCount);
    }
}