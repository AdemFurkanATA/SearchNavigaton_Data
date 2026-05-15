using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Recommendation.Data.Contexts;
using Recommendation.Shared.Models.Entities;

namespace Recommendation.Search.TfIdf;

public sealed class TfIdfIndex
{
    private readonly ConcurrentDictionary<string, Dictionary<string, double>> _index = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, int> _documentFrequency = new(StringComparer.OrdinalIgnoreCase);
    private int _documentCount;

    public async Task BuildAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
    {
        var products = await dbContext.Products.AsNoTracking().ToListAsync(cancellationToken);
        _documentCount = products.Count;

        foreach (var product in products)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var tokens = Tokenize(product.Name + " " + product.Category).ToList();
            var termCounts = tokens.GroupBy(x => x).ToDictionary(x => x.Key, x => x.Count());
            var totalTerms = tokens.Count;

            foreach (var term in termCounts.Keys)
            {
                _documentFrequency.AddOrUpdate(term, 1, (_, count) => count + 1);
            }

            var tfScores = termCounts.ToDictionary(
                kvp => kvp.Key,
                kvp => (double)kvp.Value / totalTerms);

            _index[product.Id] = tfScores;
        }
    }

    public IReadOnlyList<(Product product, double score)> Search(
        IReadOnlyList<Product> products,
        IReadOnlyList<string> queryTokens)
    {
        var results = new List<(Product product, double score)>();

        foreach (var product in products)
        {
            if (!_index.TryGetValue(product.Id, out var tfScores))
            {
                continue;
            }

            double score = 0;
            foreach (var token in queryTokens)
            {
                if (!tfScores.TryGetValue(token, out var tf))
                {
                    continue;
                }

                var idf = ComputeIdf(token);
                score += tf * idf;
            }

            if (score > 0)
            {
                results.Add((product, score));
            }
        }

        return results;
    }

    private double ComputeIdf(string term)
    {
        if (!_documentFrequency.TryGetValue(term, out var docCount) || docCount == 0)
        {
            return 0;
        }

        return Math.Log((double)_documentCount / docCount);
    }

    private static IEnumerable<string> Tokenize(string input)
    {
        return input
            .ToLowerInvariant()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}
