using FluentAssertions;
using Moq;
using Recommendation.API.Services;
using StackExchange.Redis;

namespace Recommendation.API.Tests.Services;

public sealed class BrowsingHistoryServiceTests
{
    [Fact]
    public async Task GetLastTenProducts_ReturnsSortedByDateDescending()
    {
        var redis = new Mock<IDatabase>();
        redis.Setup(x => x.SortedSetRangeByRankAsync("browsing_history:user-120", 0, 9, Order.Descending, CommandFlags.None))
            .ReturnsAsync(["product-5", "product-2", "product-1"]);

        var service = new BrowsingHistoryService(redis.Object);
        var result = await service.GetLastTenProductsAsync("user-120");

        result.Should().Equal("product-5", "product-2", "product-1");
    }

    [Fact]
    public async Task DeleteProduct_RemovesFromSortedSet()
    {
        var redis = new Mock<IDatabase>();
        redis.Setup(x => x.SortedSetRemoveAsync("browsing_history:user-120", "product-1", CommandFlags.None))
            .ReturnsAsync(true);

        var service = new BrowsingHistoryService(redis.Object);
        var removed = await service.DeleteProductAsync("user-120", "product-1");

        removed.Should().BeTrue();
    }

    [Fact]
    public async Task AddProduct_KeepsMaxTenItems()
    {
        var redis = new Mock<IDatabase>();
        var service = new ViewStream.Worker.Services.BrowsingHistoryService(redis.Object);

        await service.AddAsync("user-120", "product-1", DateTime.UtcNow);

        redis.Verify(x => x.SortedSetAddAsync("browsing_history:user-120", "product-1", It.IsAny<double>(), SortedSetWhen.Always, CommandFlags.None), Times.Once);
        redis.Verify(x => x.SortedSetRemoveRangeByRankAsync("browsing_history:user-120", 0, -11, CommandFlags.None), Times.Once);
    }
}
