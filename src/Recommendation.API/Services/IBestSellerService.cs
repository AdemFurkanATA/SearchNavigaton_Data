using Recommendation.API.Models;

namespace Recommendation.API.Services;

public interface IBestSellerService
{
    Task<BestSellerResponse> GetBestSellersAsync(string userId, CancellationToken cancellationToken = default);
}
