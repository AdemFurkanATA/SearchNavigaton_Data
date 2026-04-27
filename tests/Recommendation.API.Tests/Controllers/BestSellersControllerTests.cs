using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Recommendation.API.Controllers;
using Recommendation.API.Models;
using Recommendation.API.Services;

namespace Recommendation.API.Tests.Controllers;

public sealed class BestSellersControllerTests
{
    [Fact]
    public async Task GetBestSellers_ReturnsPersonalized_WhenUserHasBrowsingHistory()
    {
        var service = new Mock<IBestSellerService>();
        service.Setup(x => x.GetBestSellersAsync("user-120", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BestSellerResponse
            {
                UserId = "user-120",
                Products = ["product-1", "product-2", "product-3", "product-4", "product-5"],
                Type = "personalized"
            });

        var controller = new BestSellersController(service.Object);
        var result = await controller.GetBestSellers("user-120", CancellationToken.None);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var payload = ok.Value.Should().BeOfType<BestSellerResponse>().Subject;
        payload.Type.Should().Be("personalized");
    }

    [Fact]
    public async Task GetBestSellers_ReturnsNonPersonalized_WhenUserHasNoHistory()
    {
        var service = new Mock<IBestSellerService>();
        service.Setup(x => x.GetBestSellersAsync("user-120", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BestSellerResponse
            {
                UserId = "user-120",
                Products = ["product-1", "product-2", "product-3", "product-4", "product-5"],
                Type = "non-personalized"
            });

        var controller = new BestSellersController(service.Object);
        var result = await controller.GetBestSellers("user-120", CancellationToken.None);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var payload = ok.Value.Should().BeOfType<BestSellerResponse>().Subject;
        payload.Type.Should().Be("non-personalized");
    }

    [Fact]
    public async Task GetBestSellers_ReturnsEmptyList_WhenLessThanFiveProducts()
    {
        var service = new Mock<IBestSellerService>();
        service.Setup(x => x.GetBestSellersAsync("user-120", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BestSellerResponse
            {
                UserId = "user-120",
                Products = [],
                Type = "non-personalized"
            });

        var controller = new BestSellersController(service.Object);
        var result = await controller.GetBestSellers("user-120", CancellationToken.None);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var payload = ok.Value.Should().BeOfType<BestSellerResponse>().Subject;
        payload.Products.Should().BeEmpty();
    }
}
