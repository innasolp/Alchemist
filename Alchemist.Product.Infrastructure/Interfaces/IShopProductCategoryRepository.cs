using Alchemist.Product.Data;

namespace Alchemist.Product.Infrastructure.Interfaces;

public interface IShopProductCategoryRepository 
{
    Task<ShopProductCategory> AddShopProductCategory(long shopProductId, int shopCategoryId, CancellationToken cancellationToken = default);

    Task<List<ShopProductCategory>> GetShopProductCategories(long shopProductId, CancellationToken cancellationToken = default);

    Task<bool> CheckShopProductCategory(long shopProductId, int shopCategoryId, CancellationToken cancellationToken = default);
}
