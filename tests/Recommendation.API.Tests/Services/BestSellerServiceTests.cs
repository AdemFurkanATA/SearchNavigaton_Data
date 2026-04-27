using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Recommendation.API.Services;
using Recommendation.Data.Contexts;
using Recommendation.Shared.Models.Entities;

namespace Recommendation.API.Tests.Services;

public sealed class BestSellerServiceTests
{
    [Fact]
    public async Task GetBestSellers_ExtractsAtMostThreeCategories()
    {
        var db = BuildDbContext();
        db.Products.AddRange(
            new Product { Id = "p1", Category = "electronics", Name = "n1" },
            new Product { Id = "p2", Category = "electronics", Name = "n2" },
            new Product { Id = "p3", Category = "fashion", Name = "n3" },
            new Product { Id = "p4", Category = "home", Name = "n4" },
            new Product { Id = "p5", Category = "books", Name = "n5" });

        db.BestsellersByCategory.AddRange(
            new BestSellerByCategory { Category = "electronics", ProductId = "x1", Rank = 1, BuyerCount = 100 },
            new BestSellerByCategory { Category = "fashion", ProductId = "x2", Rank = 1, BuyerCount = 95 },
            new BestSellerByCategory { Category = "home", ProductId = "x3", Rank = 1, BuyerCount = 90 },
            new BestSellerByCategory { Category = "books", ProductId = "x4", Rank = 1, BuyerCount = 85 },
            new BestSellerByCategory { Category = "electronics", ProductId = "x5", Rank = 2, BuyerCount = 80 },
            new BestSellerByCategory { Category = "fashion", ProductId = "x6", Rank = 2, BuyerCount = 75 });
        await db.SaveChangesAsync();

        var history = new Mock<IBrowsingHistoryService>();
        history.Setup(x => x.GetLastTenProductsAsync("u1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(["p1", "p2", "p3", "p4", "p5"]);

        var service = new BestSellerService(db, history.Object);
        var result = await service.GetBestSellersAsync("u1");

        result.Products.Should().NotContain("x3");
    }

    [Fact]
    public async Task GetBestSellers_ReturnsEmptyList_WhenResultLessThanFive()
    {
        var db = BuildDbContext();
        db.BestsellersGeneral.AddRange(
            new BestSellerGeneral { ProductId = "x1", Rank = 1, BuyerCount = 100 },
            new BestSellerGeneral { ProductId = "x2", Rank = 2, BuyerCount = 95 },
            new BestSellerGeneral { ProductId = "x3", Rank = 3, BuyerCount = 90 },
            new BestSellerGeneral { ProductId = "x4", Rank = 4, BuyerCount = 85 });
        await db.SaveChangesAsync();

        var history = new Mock<IBrowsingHistoryService>();
        history.Setup(x => x.GetLastTenProductsAsync("u1", It.IsAny<CancellationToken>())).ReturnsAsync([]);

        var service = new BestSellerService(db, history.Object);
        var result = await service.GetBestSellersAsync("u1");

        result.Products.Should().BeEmpty();
    }

    [Fact]
    public async Task GetBestSellers_FallsBackToGeneral_WhenNoHistory()
    {
        var db = BuildDbContext();
        db.BestsellersGeneral.AddRange(
            new BestSellerGeneral { ProductId = "x1", Rank = 1, BuyerCount = 100 },
            new BestSellerGeneral { ProductId = "x2", Rank = 2, BuyerCount = 95 },
            new BestSellerGeneral { ProductId = "x3", Rank = 3, BuyerCount = 90 },
            new BestSellerGeneral { ProductId = "x4", Rank = 4, BuyerCount = 85 },
            new BestSellerGeneral { ProductId = "x5", Rank = 5, BuyerCount = 80 });
        await db.SaveChangesAsync();

        var history = new Mock<IBrowsingHistoryService>();
        history.Setup(x => x.GetLastTenProductsAsync("u1", It.IsAny<CancellationToken>())).ReturnsAsync([]);

        var service = new BestSellerService(db, history.Object);
        var result = await service.GetBestSellersAsync("u1");

        result.Type.Should().Be("non-personalized");
        result.Products.Should().HaveCount(5);
    }

    private static ApplicationDbContext BuildDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
