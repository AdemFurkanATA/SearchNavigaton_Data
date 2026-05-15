namespace Recommendation.Search.Models;

public sealed record SearchResult(string ProductId, string Name, string Category, double Score);
