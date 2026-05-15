using FluentAssertions;
using Recommendation.Search.Models;
using Recommendation.Search.Services;

namespace Recommendation.Search.Tests;

public sealed class SearchCacheServiceTests
{
    [Fact]
    public void GetOrAdd_ReturnsCachedValue_WhenNotExpired()
    {
        var cache = new SearchCacheService();
        var first = cache.GetOrAdd("key", () => new List<SearchResult>
        {
            new("p1", "name", "cat", 1.0)
        }, TimeSpan.FromMinutes(1));

        var second = cache.GetOrAdd("key", () => new List<SearchResult>
        {
            new("p2", "name", "cat", 1.0)
        }, TimeSpan.FromMinutes(1));

        second.Should().BeEquivalentTo(first);
    }
}
