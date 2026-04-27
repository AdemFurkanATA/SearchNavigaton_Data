using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Recommendation.Shared.Models;
using ViewStream.Worker.Config;
using ViewStream.Worker.Services;

namespace ViewStream.Worker.Workers;

public sealed class KafkaConsumerWorker(
    IOptions<KafkaOptions> kafkaOptions,
    IServiceScopeFactory scopeFactory,
    ILogger<KafkaConsumerWorker> logger) : BackgroundService
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = kafkaOptions.Value.BootstrapServers,
            GroupId = kafkaOptions.Value.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        consumer.Subscribe(kafkaOptions.Value.Topic);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var message = consumer.Consume(stoppingToken);
                var payload = JsonSerializer.Deserialize<ProductViewEvent>(message.Message.Value, JsonSerializerOptions);

                if (payload is null || string.IsNullOrWhiteSpace(payload.UserId) || string.IsNullOrWhiteSpace(payload.Properties.ProductId))
                {
                    consumer.Commit(message);
                    continue;
                }

                using var scope = scopeFactory.CreateScope();
                var browsingHistoryService = scope.ServiceProvider.GetRequiredService<IBrowsingHistoryService>();
                await browsingHistoryService.AddAsync(payload.UserId, payload.Properties.ProductId, payload.Timestamp, stoppingToken);
                consumer.Commit(message);
            }
            catch (ConsumeException ex)
            {
                logger.LogError(ex, "Error while consuming product view event.");
            }
        }
    }
}
