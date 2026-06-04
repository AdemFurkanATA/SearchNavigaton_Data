using Microsoft.Extensions.Options;
using Recommendation.Search.Models;
using Recommendation.Search.Options;
using Recommendation.Search.Services;
using Recommendation.Search.TfIdf;

namespace Recommendation.Search.Strategies;

public sealed class TfIdfSearchStrategy(
    SearchCacheService cacheService,
    IOptions<SearchOptions> options,
    TfIdfIndex index) : ISearchStrategy
{
    public string Name => "tfidf";

    public Task<IReadOnlyList<SearchResult>> SearchAsync(string query, int limit, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var normalized = query.Trim().ToLowerInvariant();
        var cacheKey = $"tfidf:{normalized}:{limit}";

        var results = cacheService.GetOrAdd(cacheKey, () =>
        {
            var tokens = Tokenize(normalized).ToList();
            var scores = index.Search(tokens);

            return scores
                .OrderByDescending(x => x.score)
                .Select(x => new SearchResult(x.product.Id, x.product.Name, x.product.Category, x.score))
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
