using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Recommendation.API.Controllers;
using Recommendation.API.Models;
using Recommendation.API.Services;

namespace Recommendation.API.Tests.Controllers;

public sealed class BrowsingHistoryControllerTests
{
    [Fact]
    public async Task GetBrowsingHistory_ReturnsOk_WhenUserExists()
    {
        var service = new Mock<IBrowsingHistoryService>();
        service.Setup(x => x.GetLastTenProductsAsync("user-120", It.IsAny<CancellationToken>()))
            .ReturnsAsync(["product-5", "product-12", "product-13", "product-14", "product-15"]);

        var controller = new BrowsingHistoryController(service.Object);
        var result = await controller.GetBrowsingHistory("user-120", CancellationToken.None);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var payload = ok.Value.Should().BeOfType<BrowsingHistoryResponse>().Subject;
        payload.UserId.Should().Be("user-120");
        payload.Products.Should().Equal("product-5", "product-12", "product-13", "product-14", "product-15");
        payload.Type.Should().Be("personalized");
    }

    [Fact]
    public async Task GetBrowsingHistory_ReturnsEmptyList_WhenUserHasNoHistory()
    {
        var service = new Mock<IBrowsingHistoryService>();
        service.Setup(x => x.GetLastTenProductsAsync("user-120", It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var controller = new BrowsingHistoryController(service.Object);
        var result = await controller.GetBrowsingHistory("user-120", CancellationToken.None);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var payload = ok.Value.Should().BeOfType<BrowsingHistoryResponse>().Subject;
        payload.Products.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteProductFromHistory_Returns204_WhenSuccessful()
    {
        var service = new Mock<IBrowsingHistoryService>();
        service.Setup(x => x.DeleteProductAsync("user-120", "product-5", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var controller = new BrowsingHistoryController(service.Object);
        var result = await controller.DeleteProductFromHistory("user-120", "product-5", CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteProductFromHistory_Returns404_WhenProductNotInHistory()
    {
        var service = new Mock<IBrowsingHistoryService>();
        service.Setup(x => x.DeleteProductAsync("user-120", "product-5", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var controller = new BrowsingHistoryController(service.Object);
        var result = await controller.DeleteProductFromHistory("user-120", "product-5", CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }
}
