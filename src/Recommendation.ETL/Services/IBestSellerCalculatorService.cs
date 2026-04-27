namespace Recommendation.ETL.Services;

public interface IBestSellerCalculatorService
{
    Task RebuildByCategoryAsync(CancellationToken cancellationToken = default);
    Task RebuildGeneralAsync(CancellationToken cancellationToken = default);
}
