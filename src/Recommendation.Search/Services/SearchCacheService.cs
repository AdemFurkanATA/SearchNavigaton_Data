using System.Collections.Concurrent;
using Recommendation.Search.Models;

namespace Recommendation.Search.Services;

public sealed class SearchCacheService
{
    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();

    public IReadOnlyList<SearchResult> GetOrAdd(string key, Func<IReadOnlyList<SearchResult>> factory, TimeSpan ttl)
    {
        var now = DateTime.UtcNow;
        // Use AddOrUpdate to ensure only one factory() call per expired entry
        var entry = _cache.AddOrUpdate(
            key,
            // Key not present – create new entry
            _ => new CacheEntry(factory(), now.Add(ttl)),
            // Key present – decide whether to refresh
            (_, existing) =>
            {
                // If the existing entry is still valid, keep it
                if (existing.Expiry > now)
                {
                    return existing;
                }
                // Otherwise replace with a fresh value
                return new CacheEntry(factory(), now.Add(ttl));
            });

        return entry.Results;
    }

    private sealed record CacheEntry(IReadOnlyList<SearchResult> Results, DateTime Expiry);
}
