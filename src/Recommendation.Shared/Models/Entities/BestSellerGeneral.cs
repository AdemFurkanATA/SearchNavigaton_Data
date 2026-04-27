namespace Recommendation.Shared.Models.Entities;

public sealed class BestSellerGeneral
{
    public int Id { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public int BuyerCount { get; set; }
    public int Rank { get; set; }
    public DateTime UpdatedAt { get; set; }
}
