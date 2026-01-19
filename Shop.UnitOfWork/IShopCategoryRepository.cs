using Alchemist.Product.Data;
using UnitOfWork;

namespace Shop.UnitOfWork;

public interface IShopCategoryRepository : IRepository<ShopCategory>
{
    Task<List<ShopCategory>> GetShopCategories(int shopId, CancellationToken cancellationToken = default);

    Task<List<ShopCategory>> GetAllCategoryChildren(int parentId, CancellationToken cancellationToken = default);

    Task<ShopCategory?> GetShopCategoryByShopIdAndItemId(int shopId, int itemId, CancellationToken cancellationToken = default);
}