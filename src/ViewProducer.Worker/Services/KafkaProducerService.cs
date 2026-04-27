using Confluent.Kafka;
using Microsoft.Extensions.Options;
using ViewProducer.Worker.Config;

namespace ViewProducer.Worker.Services;

public sealed class KafkaProducerService : IKafkaProducerService, IDisposable
{
    private readonly IProducer<Null, string> _producer;
    private readonly string _topic;

    public KafkaProducerService(IOptions<KafkaOptions> options)
    {
        _topic = options.Value.Topic;
        var config = new ProducerConfig
        {
            BootstrapServers = options.Value.BootstrapServers
        };

        _producer = new ProducerBuilder<Null, string>(config).Build();
    }

    public async Task ProduceAsync(string payload, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _producer.ProduceAsync(_topic, new Message<Null, string> { Value = payload }, cancellationToken);
    }

    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(2));
        _producer.Dispose();
    }
}
