using System.Text.Json;
using Microsoft.Extensions.Options;
using Recommendation.Shared.Models;
using ViewProducer.Worker.Config;
using ViewProducer.Worker.Models;
using ViewProducer.Worker.Services;

namespace ViewProducer.Worker.Workers;

public sealed class ProductViewProducerWorker(
    IKafkaProducerService kafkaProducerService,
    IOptions<ProducerOptions> producerOptions,
    ILogger<ProductViewProducerWorker> logger) : BackgroundService
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var filePath = producerOptions.Value.FilePath;
        var fullPath = Path.IsPathRooted(filePath)
            ? filePath
            : Path.Combine(AppContext.BaseDirectory, filePath);

        if (!File.Exists(fullPath))
        {
            logger.LogError("Product views file not found at {FilePath}", fullPath);
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            var lines = await File.ReadAllLinesAsync(fullPath, stoppingToken);
            foreach (var line in lines.Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                var input = JsonSerializer.Deserialize<ProductViewInput>(line, JsonSerializerOptions);
                if (input is null)
                {
                    continue;
                }

                var output = new ProductViewEvent
                {
                    Event = input.Event,
                    MessageId = input.MessageId,
                    UserId = input.UserId,
                    Properties = new ProductViewProperties
                    {
                        ProductId = input.Properties.ProductId
                    },
                    Context = new ProductViewContext
                    {
                        Source = input.Context.Source
                    },
                    Timestamp = DateTime.UtcNow
                };

                await kafkaProducerService.ProduceAsync(JsonSerializer.Serialize(output), stoppingToken);
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }
    }
}
