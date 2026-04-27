using StackExchange.Redis;

namespace ViewStream.Worker.Services;

public sealed class BrowsingHistoryService(IDatabase redis) : IBrowsingHistoryService
{
    private const string KeyPrefix = "browsing_history:";

    public async Task AddAsync(string userId, string productId, DateTime timestamp, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var key = GetKey(userId);
        var score = new DateTimeOffset(timestamp).ToUnixTimeSeconds();
        await redis.SortedSetAddAsync(key, productId, score);
        await redis.SortedSetRemoveRangeByRankAsync(key, 0, -11);
    }

    private static string GetKey(string userId) => $"{KeyPrefix}{userId}";
}
