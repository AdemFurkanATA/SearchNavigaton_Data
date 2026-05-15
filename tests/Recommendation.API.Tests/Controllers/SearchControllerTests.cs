using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using Recommendation.API.Controllers;
using Recommendation.API.Models;
using Recommendation.Search.Models;
using Recommendation.Search.Options;
using Recommendation.Search.Services;

namespace Recommendation.API.Tests.Controllers;

public sealed class SearchControllerTests
{
    [Fact]
    public async Task Search_ReturnsBadRequest_WhenQueryIsTooShort()
    {
        var strategy = new Mock<Recommendation.Search.Strategies.ISearchStrategy>();
        strategy.SetupGet(x => x.Name).Returns("prefix");
        var service = new SearchService(new[] { strategy.Object });
        var options = Options.Create(new SearchOptions { DefaultStrategy = "prefix", DefaultLimit = 10 });
        var controller = new SearchController(service, options);

        var result = await controller.Search("a", null, null, CancellationToken.None);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Search_ReturnsResults_WhenQueryIsValid()
    {
        var strategy = new Mock<Recommendation.Search.Strategies.ISearchStrategy>();
        strategy.SetupGet(x => x.Name).Returns("prefix");
        strategy.Setup(x => x.SearchAsync("head", 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SearchResult> { new("p1", "Headphones", "electronics", 1.0) });

        var service = new SearchService(new[] { strategy.Object });
        var options = Options.Create(new SearchOptions { DefaultStrategy = "prefix", DefaultLimit = 10 });
        var controller = new SearchController(service, options);

        var result = await controller.Search("head", null, null, CancellationToken.None);
        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var payload = ok.Value.Should().BeOfType<SearchResponse>().Subject;

        payload.Results.Should().HaveCount(1);
        payload.Results[0].ProductId.Should().Be("p1");
    }
}
