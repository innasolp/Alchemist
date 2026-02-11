using Alchemist.Product.Data;

namespace Alchemist.Product.Infrastructure.Interfaces;

public interface IShopProductRepository
{
    Task<ShopProduct?> GetShopProductByShopAndItemUrl(int shopId, string itemUrl, CancellationToken cancellationToken = default);

    Task<ShopProduct?> GetShopProductByShopAndItemId(int shopId, string itemId, CancellationToken cancellationToken = default);

    Task<ShopProduct?> GetShopProductByShopAndProductId(int shopId, long productId, CancellationToken cancellationToken = default);
}