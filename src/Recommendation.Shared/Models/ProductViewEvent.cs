using System.Text.Json.Serialization;

namespace Recommendation.Shared.Models;

public sealed class ProductViewEvent
{
    [JsonPropertyName("event")]
    public string Event { get; set; } = string.Empty;

    [JsonPropertyName("messageid")]
    public string MessageId { get; set; } = string.Empty;

    [JsonPropertyName("userid")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("properties")]
    public ProductViewProperties Properties { get; set; } = new();

    [JsonPropertyName("context")]
    public ProductViewContext Context { get; set; } = new();

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
}

public sealed class ProductViewProperties
{
    [JsonPropertyName("productid")]
    public string ProductId { get; set; } = string.Empty;
}

public sealed class ProductViewContext
{
    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;
}
