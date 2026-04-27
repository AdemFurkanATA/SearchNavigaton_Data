using StackExchange.Redis;
using ViewStream.Worker.Config;
using ViewStream.Worker.Services;
using ViewStream.Worker.Workers;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<KafkaOptions>(builder.Configuration.GetSection("Kafka"));
builder.Services.Configure<RedisOptions>(builder.Configuration.GetSection("Redis"));

builder.Services.AddSingleton<IConnectionMultiplexer>(provider =>
{
    var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<RedisOptions>>().Value;
    return ConnectionMultiplexer.Connect(options.ConnectionString);
});

builder.Services.AddSingleton<IDatabase>(provider => provider.GetRequiredService<IConnectionMultiplexer>().GetDatabase());
builder.Services.AddScoped<IBrowsingHistoryService, BrowsingHistoryService>();
builder.Services.AddHostedService<KafkaConsumerWorker>();

var host = builder.Build();
host.Run();
