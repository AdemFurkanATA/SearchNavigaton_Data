using Microsoft.EntityFrameworkCore;
using Recommendation.API.Models;
using Recommendation.Data.Contexts;

namespace Recommendation.API.Services;

public sealed class BestSellerService(ApplicationDbContext dbContext, IBrowsingHistoryService browsingHistoryService) : IBestSellerService
{
    private const int MinimumProductCount = 5;
    private const int MaxCategories = 3;

    public async Task<BestSellerResponse> GetBestSellersAsync(string userId, CancellationToken cancellationToken = default)
    {
        var history = await browsingHistoryService.GetLastTenProductsAsync(userId, cancellationToken);

        if (history.Count == 0)
        {
            var products = await dbContext.BestsellersGeneral
                .OrderBy(x => x.Rank)
                .ThenByDescending(x => x.BuyerCount)
                .Select(x => x.ProductId)
                .Take(10)
                .ToListAsync(cancellationToken);

            return BuildResponse(userId, products, "non-personalized");
        }

        var productCategoryMap = await dbContext.Products
            .Where(p => history.Contains(p.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Category, cancellationToken);

        var topCategories = history
            .Where(productCategoryMap.ContainsKey)
            .Select(productId => productCategoryMap[productId])
            .GroupBy(x => x)
            .OrderByDescending(x => x.Count())
            .ThenBy(x => x.Key)
            .Take(MaxCategories)
            .Select(x => x.Key)
            .ToList();

        if (topCategories.Count == 0)
        {
            return BuildResponse(userId, [], "personalized");
        }

        var categoryPriority = topCategories
            .Select((category, index) => new { category, index })
            .ToDictionary(x => x.category, x => x.index);

        var productIds = await dbContext.BestsellersByCategory
            .Where(x => topCategories.Contains(x.Category))
            .Select(x => new { x.Category, x.Rank, x.BuyerCount, x.ProductId })
            .ToListAsync(cancellationToken);

        var orderedDistinct = productIds
            .OrderBy(x => categoryPriority[x.Category])
            .ThenBy(x => x.Rank)
            .ThenByDescending(x => x.BuyerCount)
            .Select(x => x.ProductId)
            .Distinct()
            .Take(10)
            .ToList();

        return BuildResponse(userId, orderedDistinct, "personalized");
    }

    private static BestSellerResponse BuildResponse(string userId, List<string> products, string type)
    {
        return new BestSellerResponse
        {
            UserId = userId,
            Type = type,
            Products = products.Count < MinimumProductCount ? [] : products
        };
    }
}
