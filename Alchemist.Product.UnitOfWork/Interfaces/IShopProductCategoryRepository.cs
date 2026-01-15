using Alchemist.Product.Data;
using Alchemist.Product.Interfaces;
using UnitOfWork;

namespace Alchemist.Product.UnitOfWork.Interfaces;

public interface IShopProductCategoryRepository : IRepository<ShopProductCategory>
{
    Task<IShopProductCategory> AddShopProductCategory(long shopProductId, int shopCategoryId, CancellationToken cancellationToken = default);

    Task<List<ShopProductCategory>> GetShopProductCategories(long shopProductId, CancellationToken cancellationToken = default);

    Task<bool> CheckShopProductCategory(long shopProductId, int shopCategoryId, CancellationToken cancellationToken = default);
}
