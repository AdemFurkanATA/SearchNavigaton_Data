using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Recommendation.Search.Options;
using Recommendation.Search.Services;
using Recommendation.Search.Strategies;
using Recommendation.Search.TfIdf;

namespace Recommendation.Search.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRecommendationSearch(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SearchOptions>(configuration.GetSection("Search"));
        services.AddSingleton<SearchCacheService>();
        services.AddSingleton<TfIdfIndex>();
        services.AddHostedService<TfIdfIndexHostedService>();

        services.AddSingleton<PrefixTreeSearchStrategy>();
        services.AddHostedService(provider => provider.GetRequiredService<PrefixTreeSearchStrategy>());

        services.AddSingleton<ISearchStrategy>(provider => provider.GetRequiredService<PrefixTreeSearchStrategy>());
        services.AddSingleton<ISearchStrategy, FuzzySearchStrategy>();
        services.AddSingleton<ISearchStrategy, TfIdfSearchStrategy>();
        services.AddSingleton<SearchService>();

        return services;
    }
}
