namespace Recommendation.API.Services;

public interface IBrowsingHistoryService
{
    Task<IReadOnlyList<string>> GetLastTenProductsAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> DeleteProductAsync(string userId, string productId, CancellationToken cancellationToken = default);
}
