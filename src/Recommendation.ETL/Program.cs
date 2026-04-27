using Microsoft.EntityFrameworkCore;
using Quartz;
using Recommendation.Data.Contexts;
using Recommendation.ETL.Jobs;
using Recommendation.ETL.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IBestSellerCalculatorService, BestSellerCalculatorService>();

var cronSchedule = builder.Configuration["Quartz:BestSellersCronSchedule"] ?? "0 0 2 * * ?";

builder.Services.AddQuartz(options =>
{
    var byCategoryKey = new JobKey("best-sellers-by-category");
    var generalKey = new JobKey("best-sellers-general");

    options.AddJob<BestSellersByCategoryJob>(job => job.WithIdentity(byCategoryKey));
    options.AddTrigger(trigger => trigger
        .ForJob(byCategoryKey)
        .WithIdentity("best-sellers-by-category-trigger")
        .WithCronSchedule(cronSchedule));

    options.AddJob<GeneralBestSellersJob>(job => job.WithIdentity(generalKey));
    options.AddTrigger(trigger => trigger
        .ForJob(generalKey)
        .WithIdentity("best-sellers-general-trigger")
        .WithCronSchedule(cronSchedule));
});

builder.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

var host = builder.Build();
host.Run();
