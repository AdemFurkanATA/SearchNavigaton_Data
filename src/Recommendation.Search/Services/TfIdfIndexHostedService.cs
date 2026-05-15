using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Recommendation.Data.Contexts;
using Recommendation.Search.TfIdf;

namespace Recommendation.Search.Services;

public sealed class TfIdfIndexHostedService(
    ApplicationDbContext dbContext,
    TfIdfIndex index,
    ILogger<TfIdfIndexHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await index.BuildAsync(dbContext, stoppingToken);
        logger.LogInformation("TF-IDF index initialized.");
    }
}
