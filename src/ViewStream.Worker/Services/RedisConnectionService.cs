using StackExchange.Redis;

namespace ViewStream.Worker.Services;

public sealed class RedisConnectionService : IAsyncDisposable
{
    private readonly Lazy<Task<ConnectionMultiplexer>> _lazyConnection;

    public RedisConnectionService(string connectionString)
    {
        _lazyConnection = new Lazy<Task<ConnectionMultiplexer>>(() => ConnectionMultiplexer.ConnectAsync(connectionString));
    }

    public async Task<IDatabase> GetDatabaseAsync()
    {
        var connection = await _lazyConnection.Value;
        return connection.GetDatabase();
    }

    public async ValueTask DisposeAsync()
    {
        if (_lazyConnection.IsValueCreated)
        {
            var connection = await _lazyConnection.Value;
            await connection.CloseAsync();
            connection.Dispose();
        }
    }
}
