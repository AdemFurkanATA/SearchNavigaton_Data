using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Recommendation.Data.Contexts;
using Recommendation.Shared.Models.Entities;

namespace Recommendation.Search.TfIdf;

public sealed class TfIdfIndex
{
    private const int PageSize = 500;
    private static readonly IndexSnapshot EmptySnapshot = new(
        new Dictionary<string, Posting[]>(StringComparer.OrdinalIgnoreCase).ToFrozenDictionary(StringComparer.OrdinalIgnoreCase),
        new Dictionary<string, Product>(StringComparer.OrdinalIgnoreCase).ToFrozenDictionary(StringComparer.OrdinalIgnoreCase));

    private IndexSnapshot _snapshot = EmptySnapshot;

    public async Task BuildAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
    {
        var tfByTerm = new ConcurrentDictionary<string, ConcurrentDictionary<string, double>>(StringComparer.OrdinalIgnoreCase);
        var documentFrequency = new ConcurrentDictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var productLookup = new ConcurrentDictionary<string, Product>(StringComparer.OrdinalIgnoreCase);
        var documentCount = 0;
        var page = 0;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var products = await dbContext.Products
                .AsNoTracking()
                .OrderBy(x => x.Id)
                .Skip(page * PageSize)
                .Take(PageSize)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            if (products.Count == 0)
            {
                break;
            }

            await Parallel.ForEachAsync(
                products,
                new ParallelOptions
                {
                    CancellationToken = cancellationToken,
                    MaxDegreeOfParallelism = Environment.ProcessorCount
                },
                (product, ct) =>
                {
                    ct.ThrowIfCancellationRequested();

                    var tokens = Tokenize($"{product.Name} {product.Category}");
                    var copy = new Product
                    {
                        Id = product.Id,
                        Name = product.Name,
                        Category = product.Category,
                        Price = product.Price
                    };
                    productLookup[product.Id] = copy;

                    Interlocked.Increment(ref documentCount);
                    if (tokens.Length == 0)
                    {
                        return ValueTask.CompletedTask;
                    }

                    var termCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                    foreach (var token in tokens)
                    {
                        termCounts[token] = termCounts.TryGetValue(token, out var currentCount)
                            ? currentCount + 1
                            : 1;
                    }

                    var totalTerms = tokens.Length;
                    foreach (var (term, count) in termCounts)
                    {
                        documentFrequency.AddOrUpdate(term, 1, static (_, currentCount) => currentCount + 1);

                        var tf = (double)count / totalTerms;
                        var byDocument = tfByTerm.GetOrAdd(
                            term,
                            static _ => new ConcurrentDictionary<string, double>(StringComparer.OrdinalIgnoreCase));
                        byDocument[product.Id] = tf;
                    }

                    return ValueTask.CompletedTask;
                })
                .ConfigureAwait(false);

            page++;
        }

        var finalizedInvertedIndex = new Dictionary<string, Posting[]>(StringComparer.OrdinalIgnoreCase);
        if (documentCount > 0)
        {
            foreach (var (term, byDocument) in tfByTerm)
            {
                if (!documentFrequency.TryGetValue(term, out var docFrequency) || docFrequency <= 0)
                {
                    continue;
                }

                var idf = Math.Log((double)documentCount / docFrequency);
                if (idf <= 0)
                {
                    continue;
                }

                var postings = byDocument
                    .Select(x => new Posting(x.Key, x.Value * idf))
                    .Where(x => x.Score > 0)
                    .OrderByDescending(x => x.Score)
                    .ToArray();

                if (postings.Length > 0)
                {
                    finalizedInvertedIndex[term] = postings;
                }
            }
        }

        var snapshot = new IndexSnapshot(
            finalizedInvertedIndex.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase),
            productLookup.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase));

        Volatile.Write(ref _snapshot, snapshot);
    }

    public IReadOnlyList<(Product product, double score)> Search(IReadOnlyList<string> queryTokens)
    {
        if (queryTokens.Count == 0)
        {
            return Array.Empty<(Product product, double score)>();
        }

        var snapshot = Volatile.Read(ref _snapshot);
        if (snapshot.InvertedIndex.Count == 0)
        {
            return Array.Empty<(Product product, double score)>();
        }

        var scores = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        foreach (var token in queryTokens)
        {
            if (!snapshot.InvertedIndex.TryGetValue(token, out var postings))
            {
                continue;
            }

            foreach (var posting in postings)
            {
                scores[posting.ProductId] = scores.TryGetValue(posting.ProductId, out var currentScore)
                    ? currentScore + posting.Score
                    : posting.Score;
            }
        }

        if (scores.Count == 0)
        {
            return Array.Empty<(Product product, double score)>();
        }

        var results = new List<(Product product, double score)>(scores.Count);
        foreach (var (productId, score) in scores)
        {
            if (snapshot.ProductLookup.TryGetValue(productId, out var product))
            {
                results.Add((product, score));
            }
        }

        return results;
    }

    private static string[] Tokenize(string input)
    {
        return input
            .ToLowerInvariant()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    private sealed record IndexSnapshot(
        FrozenDictionary<string, Posting[]> InvertedIndex,
        FrozenDictionary<string, Product> ProductLookup);

    private readonly record struct Posting(string ProductId, double Score);
}
