using StackExchange.Redis;

namespace Recommendation.API.Services;

public sealed class BrowsingHistoryService(IDatabase redis) : IBrowsingHistoryService
{
    private const string KeyPrefix = "browsing_history:";

    public async Task<IReadOnlyList<string>> GetLastTenProductsAsync(string userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var key = GetKey(userId);
        var values = await redis.SortedSetRangeByRankAsync(key, 0, 9, Order.Descending);
        return values
            .Select(x => x.ToString())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!)
            .ToList();
    }

    public async Task<bool> DeleteProductAsync(string userId, string productId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var key = GetKey(userId);
        return await redis.SortedSetRemoveAsync(key, productId);
    }

    private static string GetKey(string userId) => $"{KeyPrefix}{userId}";
}
