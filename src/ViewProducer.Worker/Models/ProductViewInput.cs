using System.Text.Json.Serialization;

namespace ViewProducer.Worker.Models;

public sealed class ProductViewInput
{
    [JsonPropertyName("event")]
    public string Event { get; set; } = string.Empty;

    [JsonPropertyName("messageid")]
    public string MessageId { get; set; } = string.Empty;

    [JsonPropertyName("userid")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("properties")]
    public ProductViewInputProperties Properties { get; set; } = new();

    [JsonPropertyName("context")]
    public ProductViewInputContext Context { get; set; } = new();
}

public sealed class ProductViewInputProperties
{
    [JsonPropertyName("productid")]
    public string ProductId { get; set; } = string.Empty;
}

public sealed class ProductViewInputContext
{
    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;
}
