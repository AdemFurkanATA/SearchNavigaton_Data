using Microsoft.AspNetCore.Mvc;
using Recommendation.API.Models;
using Recommendation.API.Services;

namespace Recommendation.API.Controllers;

[ApiController]
[Route("browsing-history")]
public sealed class BrowsingHistoryController(IBrowsingHistoryService browsingHistoryService) : ControllerBase
{
    private const int MinimumProductCount = 5;

    [HttpGet("{userId}")]
    public async Task<ActionResult<BrowsingHistoryResponse>> GetBrowsingHistory(string userId, CancellationToken cancellationToken)
    {
        var products = await browsingHistoryService.GetLastTenProductsAsync(userId, cancellationToken);
        return Ok(new BrowsingHistoryResponse
        {
            UserId = userId,
            Type = "personalized",
            Products = products.Count < MinimumProductCount ? [] : products.ToList()
        });
    }

    [HttpDelete("{userId}/{productId}")]
    public async Task<IActionResult> DeleteProductFromHistory(string userId, string productId, CancellationToken cancellationToken)
    {
        var removed = await browsingHistoryService.DeleteProductAsync(userId, productId, cancellationToken);
        return removed ? NoContent() : NotFound();
    }
}
