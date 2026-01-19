using Alchemist.Product.Data;
using UnitOfWork;

namespace Alchemist.Product.UnitOfWork.Interfaces;

public interface IShopProductRepository : IRepository<ShopProduct>
{
    Task<ShopProduct?> GetShopProductByShopAndItemUrl(int shopId, string itemUrl, CancellationToken cancellationToken = default);

    Task<ShopProduct?> GetShopProductByShopAndItemId(int shopId, string itemId, CancellationToken cancellationToken = default);

    Task<ShopProduct?> GetShopProductByShopAndProductId(int shopId, long productId, CancellationToken cancellationToken = default);
}