using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Recommendation.API.Models;
using Recommendation.Search.Options;
using Recommendation.Search.Services;

namespace Recommendation.API.Controllers;

[ApiController]
[Route("search")]
public sealed class SearchController(
    SearchService searchService,
    IOptions<SearchOptions> options) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<SearchResponse>> Search(
        [FromQuery(Name = "q")] string query,
        [FromQuery(Name = "strategy")] string? strategy,
        [FromQuery(Name = "limit")] int? limit,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Trim().Length < 2)
        {
            return BadRequest("Query must be at least 2 characters.");
        }

        var selectedStrategy = string.IsNullOrWhiteSpace(strategy)
            ? options.Value.DefaultStrategy
            : strategy;
        var resultLimit = limit.GetValueOrDefault(options.Value.DefaultLimit);

        var stopwatch = Stopwatch.StartNew();
        var results = await searchService.SearchAsync(selectedStrategy!, query, resultLimit, cancellationToken);
        stopwatch.Stop();

        return Ok(new SearchResponse
        {
            Query = query,
            Strategy = selectedStrategy!,
            Results = results,
            ElapsedMilliseconds = stopwatch.ElapsedMilliseconds
        });
    }
}
