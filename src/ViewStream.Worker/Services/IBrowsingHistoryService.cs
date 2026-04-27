namespace ViewStream.Worker.Services;

public interface IBrowsingHistoryService
{
    Task AddAsync(string userId, string productId, DateTime timestamp, CancellationToken cancellationToken = default);
}
