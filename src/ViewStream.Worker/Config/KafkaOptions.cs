namespace ViewStream.Worker.Config;

public sealed class KafkaOptions
{
    public string BootstrapServers { get; set; } = "localhost:9092";
    public string Topic { get; set; } = "product-views";
    public string GroupId { get; set; } = "recommendation-stream-group";
}
