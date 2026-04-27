namespace ViewProducer.Worker.Services;

public interface IKafkaProducerService
{
    Task ProduceAsync(string payload, CancellationToken cancellationToken = default);
}
