using Moq;
using Quartz;
using Recommendation.ETL.Jobs;
using Recommendation.ETL.Services;

namespace Recommendation.ETL.Tests.Jobs;

public sealed class BestSellerJobsTests
{
    [Fact]
    public async Task BestSellersByCategoryJob_CallsCalculator()
    {
        var service = new Mock<IBestSellerCalculatorService>();
        var context = new Mock<IJobExecutionContext>();
        var job = new BestSellersByCategoryJob(service.Object);

        await job.Execute(context.Object);

        service.Verify(x => x.RebuildByCategoryAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GeneralBestSellersJob_CallsCalculator()
    {
        var service = new Mock<IBestSellerCalculatorService>();
        var context = new Mock<IJobExecutionContext>();
        var job = new GeneralBestSellersJob(service.Object);

        await job.Execute(context.Object);

        service.Verify(x => x.RebuildGeneralAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
