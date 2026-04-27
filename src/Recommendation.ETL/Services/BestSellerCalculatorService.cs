using Microsoft.EntityFrameworkCore;
using Recommendation.Data.Contexts;

namespace Recommendation.ETL.Services;

public sealed class BestSellerCalculatorService(ApplicationDbContext dbContext) : IBestSellerCalculatorService
{
    public async Task RebuildByCategoryAsync(CancellationToken cancellationToken = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        await dbContext.Database.ExecuteSqlRawAsync("DELETE FROM bestsellers_by_category;", cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO bestsellers_by_category (category, product_id, buyer_count, rank, updated_at)
            SELECT category, product_id, buyer_count, rank, NOW()
            FROM (
                SELECT
                    p.category AS category,
                    oi.product_id AS product_id,
                    COUNT(DISTINCT o.user_id) AS buyer_count,
                    RANK() OVER (
                        PARTITION BY p.category
                        ORDER BY COUNT(DISTINCT o.user_id) DESC, oi.product_id ASC
                    ) AS rank
                FROM order_item oi
                JOIN "order" o ON o.id = oi.order_id
                JOIN product p ON p.id = oi.product_id
                WHERE o.created_at >= NOW() - INTERVAL '30 days'
                GROUP BY p.category, oi.product_id
            ) ranked
            WHERE rank <= 10;
            """,
            cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task RebuildGeneralAsync(CancellationToken cancellationToken = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        await dbContext.Database.ExecuteSqlRawAsync("DELETE FROM bestsellers_general;", cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO bestsellers_general (product_id, buyer_count, rank, updated_at)
            SELECT product_id, buyer_count, rank, NOW()
            FROM (
                SELECT
                    oi.product_id AS product_id,
                    COUNT(DISTINCT o.user_id) AS buyer_count,
                    RANK() OVER (
                        ORDER BY COUNT(DISTINCT o.user_id) DESC, oi.product_id ASC
                    ) AS rank
                FROM order_item oi
                JOIN "order" o ON o.id = oi.order_id
                WHERE o.created_at >= NOW() - INTERVAL '30 days'
                GROUP BY oi.product_id
            ) ranked
            WHERE rank <= 10;
            """,
            cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }
}
