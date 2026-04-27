namespace ViewStream.Worker.Config;

public sealed class RedisOptions
{
    public string ConnectionString { get; set; } = "localhost:6379";
}
