using Alchemist.Product.Data;
using UnitOfWork;

namespace Alchemist.Product.UnitOfWork.Interfaces;

public interface IShopProductPriceRepository : IRepository<ShopProductPrice>
{
    Task<ShopProductPrice?> GetShopProductPrice(long shopProductId, CancellationToken cancellationToken = default);
}
