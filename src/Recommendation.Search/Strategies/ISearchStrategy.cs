using Recommendation.Search.Models;

namespace Recommendation.Search.Strategies;

public interface ISearchStrategy
{
    string Name { get; }
    Task<IReadOnlyList<SearchResult>> SearchAsync(string query, int limit, CancellationToken cancellationToken);
}
