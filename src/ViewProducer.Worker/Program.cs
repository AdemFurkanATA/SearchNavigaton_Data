using ViewProducer.Worker.Config;
using ViewProducer.Worker.Services;
using ViewProducer.Worker.Workers;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<KafkaOptions>(builder.Configuration.GetSection("Kafka"));
builder.Services.Configure<ProducerOptions>(builder.Configuration.GetSection("Producer"));

builder.Services.AddSingleton<IKafkaProducerService, KafkaProducerService>();
builder.Services.AddHostedService<ProductViewProducerWorker>();

var host = builder.Build();
host.Run();
