using Quartz;
using Recommendation.ETL.Services;

namespace Recommendation.ETL.Jobs;

public sealed class GeneralBestSellersJob(IBestSellerCalculatorService calculatorService) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        await calculatorService.RebuildGeneralAsync(context.CancellationToken);
    }
}
