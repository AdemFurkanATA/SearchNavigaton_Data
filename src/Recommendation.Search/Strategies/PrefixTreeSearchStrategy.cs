using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Recommendation.Data.Contexts;
using Recommendation.Search.Models;
using Recommendation.Search.Options;
using Recommendation.Search.Services;
using Recommendation.Search.Trie;
using Recommendation.Shared.Models.Entities;

namespace Recommendation.Search.Strategies;

public sealed class PrefixTreeSearchStrategy(
    ApplicationDbContext dbContext,
    SearchCacheService cacheService,
    IOptions<SearchOptions> options,
    ILogger<PrefixTreeSearchStrategy> logger) : BackgroundService, ISearchStrategy
{
    private readonly Trie.Trie _trie = new();
    private readonly Dictionary<string, Product> _productLookup = new(StringComparer.OrdinalIgnoreCase);
    private readonly TimeSpan _cacheTtl = TimeSpan.FromMinutes(options.Value.CacheTtlMinutes);
    public string Name => "prefix";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var products = await dbContext.Products.AsNoTracking().ToListAsync(stoppingToken);
        foreach (var product in products)
        {
            _productLookup[product.Id] = product;
            foreach (var token in Tokenize(product.Name))
            {
                _trie.Insert(token, product.Id);
            }
        }

        _trie.Freeze();
        logger.LogInformation("PrefixTreeSearchStrategy initialized with {ProductCount} products", products.Count);
    }

    public Task<IReadOnlyList<SearchResult>> SearchAsync(string query, int limit, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var normalized = query.Trim().ToLowerInvariant();
        var cacheKey = $"prefix:{normalized}:{limit}";

        var results = cacheService.GetOrAdd(cacheKey, () =>
        {
            var matches = _trie.GetByPrefix(normalized);
            return matches
                .Select(id => _productLookup.TryGetValue(id, out var product)
                    ? new SearchResult(product.Id, product.Name, product.Category, 1.0)
                    : null)
                .Where(x => x is not null)
                .Select(x => x!)
                .Take(limit)
                .ToList();
        }, _cacheTtl);

        return Task.FromResult<IReadOnlyList<SearchResult>>(results);
    }

    private static IEnumerable<string> Tokenize(string input)
    {
        return input
            .ToLowerInvariant()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}
