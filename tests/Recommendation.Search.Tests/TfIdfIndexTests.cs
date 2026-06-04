using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Recommendation.Data.Contexts;
using Recommendation.Search.TfIdf;
using Recommendation.Shared.Models.Entities;

namespace Recommendation.Search.Tests;

public sealed class TfIdfIndexTests
{
    [Fact]
    public async Task BuildAsync_AssignsHigherScoreForRareTerms()
    {
        var db = BuildDbContext();
        db.Products.AddRange(
            new Product { Id = "p1", Name = "Wireless Headphones", Category = "electronics" },
            new Product { Id = "p2", Name = "Wireless Mouse", Category = "electronics" },
            new Product { Id = "p3", Name = "Organic Coffee", Category = "grocery" });
        await db.SaveChangesAsync();

        var index = new TfIdfIndex();
        await index.BuildAsync(db, CancellationToken.None);

        var results = index.Search(new[] { "organic" });
        results.Should().ContainSingle(x => x.product.Id == "p3");
    }

    [Fact]
    public async Task Search_ReturnsEmptyForNoMatches_AndRanksMoreRelevantDocumentHigher()
    {
        var db = BuildDbContext();
        db.Products.AddRange(
            new Product { Id = "p1", Name = "Organic Organic Coffee", Category = "grocery" },
            new Product { Id = "p2", Name = "Organic Tea", Category = "grocery" },
            new Product { Id = "p3", Name = "Wireless Mouse", Category = "electronics" });
        await db.SaveChangesAsync();

        var index = new TfIdfIndex();
        await index.BuildAsync(db, CancellationToken.None);

        var noMatchResults = index.Search(new[] { "nonexistent" });
        noMatchResults.Should().BeEmpty();

        var relevanceResults = index.Search(new[] { "organic" });
        relevanceResults.Should().Contain(x => x.product.Id == "p1");
        relevanceResults.Should().Contain(x => x.product.Id == "p2");

        var p1Score = relevanceResults.Single(x => x.product.Id == "p1").score;
        var p2Score = relevanceResults.Single(x => x.product.Id == "p2").score;
        p1Score.Should().BeGreaterThan(p2Score);
    }

    private static ApplicationDbContext BuildDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
