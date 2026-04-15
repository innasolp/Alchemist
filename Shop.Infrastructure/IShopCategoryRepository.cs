using Alchemist.Product.Data;

namespace Shop.Infrastructure;

public interface IShopCategoryRepository
{
    Task<List<ShopCategory>> GetShopCategories(int shopId, CancellationToken cancellationToken = default);

    Task<List<ShopCategory>> GetAllCategoryChildren(int parentId, CancellationToken cancellationToken = default);

    Task<ShopCategory?> GetShopCategoryByShopIdAndItemId(int shopId, int itemId, CancellationToken cancellationToken = default);

    Task<bool?> CheckCategoryForAncestorItem(int id, int ancestorId, CancellationToken cancellationToken = default);
}