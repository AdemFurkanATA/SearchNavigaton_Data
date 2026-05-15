using System.Collections.Concurrent;
using Recommendation.Search.Models;

namespace Recommendation.Search.Services;

public sealed class SearchCacheService
{
    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();

    public IReadOnlyList<SearchResult> GetOrAdd(string key, Func<IReadOnlyList<SearchResult>> factory, TimeSpan ttl)
    {
        var now = DateTime.UtcNow;
        var entry = _cache.GetOrAdd(key, _ => new CacheEntry(factory(), now.Add(ttl)));

        if (entry.Expiry <= now)
        {
            var refreshed = new CacheEntry(factory(), now.Add(ttl));
            _cache[key] = refreshed;
            return refreshed.Results;
        }

        return entry.Results;
    }

    private sealed record CacheEntry(IReadOnlyList<SearchResult> Results, DateTime Expiry);
}
