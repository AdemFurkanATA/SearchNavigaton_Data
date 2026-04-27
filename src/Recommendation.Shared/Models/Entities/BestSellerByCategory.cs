namespace Recommendation.Shared.Models.Entities;

public sealed class BestSellerByCategory
{
    public int Id { get; set; }
    public string Category { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public int BuyerCount { get; set; }
    public int Rank { get; set; }
    public DateTime UpdatedAt { get; set; }
}
