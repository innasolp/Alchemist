using Alchemist.Product.Data;

namespace Alchemist.Product.Infrastructure.Interfaces;

public interface IShopProductPriceRepository
{
    Task<ShopProductPrice?> GetShopProductPrice(long shopProductId, CancellationToken cancellationToken = default);
}
