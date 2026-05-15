using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Recommendation.Data.Contexts;
using Recommendation.Search.Options;
using Recommendation.Search.Services;
using Recommendation.Search.Strategies;
using Recommendation.Search.TfIdf;
using Recommendation.Shared.Models.Entities;

namespace Recommendation.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
public class SearchStrategyBenchmarks
{
    private ApplicationDbContext _dbContext = null!;
    private PrefixTreeSearchStrategy _prefix = null!;
    private FuzzySearchStrategy _fuzzy = null!;
    private TfIdfSearchStrategy _tfidf = null!;
    private TfIdfIndex _index = null!;
    private SearchCacheService _cache = null!;

    [Params(100, 1000, 10000)]
    public int ProductCount { get; set; }

    [GlobalSetup]
    public async Task Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ApplicationDbContext(options);

        var faker = new Faker();
        var products = new List<Product>();
        for (var i = 0; i < ProductCount; i++)
        {
            products.Add(new Product
            {
                Id = $"product-{i}",
                Name = faker.Commerce.ProductName(),
                Category = faker.Commerce.Categories(1)[0],
                Price = faker.Random.Decimal(1, 1000)
            });
        }

        _dbContext.Products.AddRange(products);
        await _dbContext.SaveChangesAsync();

        _cache = new SearchCacheService();
        var searchOptions = Options.Create(new SearchOptions { FuzzyMaxEditDistance = 2, CacheTtlMinutes = 5 });

        _index = new TfIdfIndex();
        await _index.BuildAsync(_dbContext, CancellationToken.None);

        _prefix = new PrefixTreeSearchStrategy(_dbContext, _cache, searchOptions, NullLogger<PrefixTreeSearchStrategy>.Instance);
        await _prefix.StartAsync(CancellationToken.None);
        _fuzzy = new FuzzySearchStrategy(_dbContext, _cache, searchOptions);
        _tfidf = new TfIdfSearchStrategy(_dbContext, _cache, searchOptions, _index);
    }

    [Benchmark]
    public Task PrefixSearch() => _prefix.SearchAsync("wire", 10, CancellationToken.None);

    [Benchmark]
    public Task FuzzySearch() => _fuzzy.SearchAsync("wire", 10, CancellationToken.None);

    [Benchmark]
    public Task TfIdfSearch() => _tfidf.SearchAsync("wire", 10, CancellationToken.None);
}
