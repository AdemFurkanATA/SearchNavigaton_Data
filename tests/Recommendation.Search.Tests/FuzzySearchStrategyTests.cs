using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Recommendation.Data.Contexts;
using Recommendation.Search.Options;
using Recommendation.Search.Services;
using Recommendation.Search.Strategies;
using Recommendation.Shared.Models.Entities;

namespace Recommendation.Search.Tests;

public sealed class FuzzySearchStrategyTests
{
    [Fact]
    public async Task SearchAsync_ReturnsMatchesWithinThreshold()
    {
        var db = BuildDbContext();
        db.Products.Add(new Product { Id = "p1", Name = "Wireless Headphones", Category = "electronics" });
        await db.SaveChangesAsync();

        var options = Microsoft.Extensions.Options.Options.Create(new SearchOptions { FuzzyMaxEditDistance = 2, CacheTtlMinutes = 1 });
        var cache = new SearchCacheService();
        var strategy = new FuzzySearchStrategy(db, cache, options);

        var results = await strategy.SearchAsync("wireles", 10, CancellationToken.None);
        results.Should().NotBeEmpty();
        results[0].ProductId.Should().Be("p1");
    }

    private static ApplicationDbContext BuildDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
