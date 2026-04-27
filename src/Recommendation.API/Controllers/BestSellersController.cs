using Microsoft.AspNetCore.Mvc;
using Recommendation.API.Models;
using Recommendation.API.Services;

namespace Recommendation.API.Controllers;

[ApiController]
[Route("best-sellers")]
public sealed class BestSellersController(IBestSellerService bestSellerService) : ControllerBase
{
    [HttpGet("{userId}")]
    public async Task<ActionResult<BestSellerResponse>> GetBestSellers(string userId, CancellationToken cancellationToken)
    {
        var response = await bestSellerService.GetBestSellersAsync(userId, cancellationToken);
        return Ok(response);
    }
}
