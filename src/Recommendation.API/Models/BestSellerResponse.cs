using System.Text.Json.Serialization;

namespace Recommendation.API.Models;

public sealed class BestSellerResponse
{
    [JsonPropertyName("user-id")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("products")]
    public List<string> Products { get; set; } = [];

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
}
