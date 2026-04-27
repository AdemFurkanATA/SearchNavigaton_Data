using Quartz;
using Recommendation.ETL.Services;

namespace Recommendation.ETL.Jobs;

public sealed class BestSellersByCategoryJob(IBestSellerCalculatorService calculatorService) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        await calculatorService.RebuildByCategoryAsync(context.CancellationToken);
    }
}
