using Microsoft.EntityFrameworkCore;
using Recommendation.API.Services;
using Recommendation.Data.Contexts;
using Recommendation.Search.Extensions;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379"));

builder.Services.AddSingleton<IDatabase>(provider =>
    provider.GetRequiredService<IConnectionMultiplexer>().GetDatabase());

builder.Services.AddScoped<IBrowsingHistoryService, BrowsingHistoryService>();
builder.Services.AddScoped<IBestSellerService, BestSellerService>();
builder.Services.AddRecommendationSearch(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
