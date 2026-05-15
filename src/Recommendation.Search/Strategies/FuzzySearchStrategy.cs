using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Recommendation.Data.Contexts;
using Recommendation.Search.Models;
using Recommendation.Search.Options;
using Recommendation.Search.Services;

namespace Recommendation.Search.Strategies;

public sealed class FuzzySearchStrategy(
    ApplicationDbContext dbContext,
    SearchCacheService cacheService,
    IOptions<SearchOptions> options) : ISearchStrategy
{
    public string Name => "fuzzy";

    public Task<IReadOnlyList<SearchResult>> SearchAsync(string query, int limit, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var normalized = query.Trim().ToLowerInvariant();
        var cacheKey = $"fuzzy:{normalized}:{limit}";

        var results = cacheService.GetOrAdd(cacheKey, () =>
        {
            var maxDistance = options.Value.FuzzyMaxEditDistance;
            var products = dbContext.Products.AsNoTracking().ToList();
            var results = new List<SearchResult>();

            foreach (var product in products)
            {
                foreach (var token in Tokenize(product.Name))
                {
                    var distance = LevenshteinDistance.Compute(normalized, token);
                    if (distance > maxDistance)
                    {
                        continue;
                    }

                    var score = 1.0 - (double)distance / Math.Max(normalized.Length, token.Length);
                    results.Add(new SearchResult(product.Id, product.Name, product.Category, score));
                }
            }

            return results
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.Name)
                .Take(limit)
                .ToList();
        }, TimeSpan.FromMinutes(options.Value.CacheTtlMinutes));

        return Task.FromResult<IReadOnlyList<SearchResult>>(results);
    }

    private static IEnumerable<string> Tokenize(string input)
    {
        return input
            .ToLowerInvariant()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}
