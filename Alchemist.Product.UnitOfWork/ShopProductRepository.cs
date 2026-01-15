using Alchemist.Product.Data;
using Alchemist.Product.UnitOfWork.Interfaces;
using Microsoft.EntityFrameworkCore;
using UnitOfWork;

namespace Alchemist.Product.UnitOfWork;

public class ShopProductRepository(AlchemyContext context) : Repository<ShopProduct, AlchemyContext>(context), IShopProductRepository
{
    public Task<ShopProduct?> GetShopProductByShopAndItemUrl(int shopId, string itemUrl, CancellationToken cancellationToken = default)
    {
        return Context.ShopProducts.Where(sp => sp.ShopId == shopId && sp.ApiUrl.Trim() == itemUrl.Trim()).FirstOrDefaultAsync(cancellationToken);
    }

    public Task<ShopProduct?> GetShopProductByShopAndItemId(int shopId, string itemId, CancellationToken cancellationToken = default)
    {
        return Context.ShopProducts.Where(sp => sp.ShopId == shopId && sp.ItemId.Trim() == itemId.Trim()).FirstOrDefaultAsync(cancellationToken);
    }

    public Task<ShopProduct?> GetShopProductByShopAndProductId(int shopId, long productId, CancellationToken cancellationToken = default)
    {
        return Context.ShopProducts.Where(sp => sp.ShopId == shopId && sp.ProductId == productId).FirstOrDefaultAsync(cancellationToken);
    }
}