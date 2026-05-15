using Recommendation.Search.Models;
using Recommendation.Search.Strategies;

namespace Recommendation.Search.Services;

public sealed class SearchService(IEnumerable<ISearchStrategy> strategies)
{
    private readonly IReadOnlyDictionary<string, ISearchStrategy> _strategies = strategies
        .ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);

    public Task<IReadOnlyList<SearchResult>> SearchAsync(
        string strategyName,
        string query,
        int limit,
        CancellationToken cancellationToken)
    {
        if (!_strategies.TryGetValue(strategyName, out var strategy))
        {
            throw new ArgumentException($"Unknown strategy: {strategyName}", nameof(strategyName));
        }

        return strategy.SearchAsync(query, limit, cancellationToken);
    }
}
