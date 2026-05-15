using System.Text.Json.Serialization;
using Recommendation.Search.Models;

namespace Recommendation.API.Models;

public sealed class SearchResponse
{
    [JsonPropertyName("query")]
    public string Query { get; set; } = string.Empty;

    [JsonPropertyName("strategy")]
    public string Strategy { get; set; } = string.Empty;

    [JsonPropertyName("results")]
    public IReadOnlyList<SearchResult> Results { get; set; } = Array.Empty<SearchResult>();

    [JsonPropertyName("elapsed-ms")]
    public long ElapsedMilliseconds { get; set; }
}
