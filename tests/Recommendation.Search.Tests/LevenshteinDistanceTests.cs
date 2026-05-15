using FluentAssertions;
using Recommendation.Search.Strategies;

namespace Recommendation.Search.Tests;

public sealed class LevenshteinDistanceTests
{
    [Fact]
    public void Compute_ReturnsKnownDistance()
    {
        var distance = LevenshteinDistance.Compute("kitten", "sitting");
        distance.Should().Be(3);
    }
}
