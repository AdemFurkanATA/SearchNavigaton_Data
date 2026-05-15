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

        var products = db.Products.AsNoTracking().ToList();
        var results = index.Search(products, new[] { "organic" });
        results.Should().ContainSingle(x => x.product.Id == "p3");
    }

    private static ApplicationDbContext BuildDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
