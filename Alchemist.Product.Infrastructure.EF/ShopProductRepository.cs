using Alchemist.Product.Data;
using Alchemist.Product.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Alchemist.Product.Infrastructure.EF;

public class ShopProductRepository(AlchemyContext context) : IShopProductRepository
{
    private readonly AlchemyContext _context = context;

    public Task<ShopProduct?> GetShopProductByShopAndItemUrl(int shopId, string itemUrl, CancellationToken cancellationToken = default)
    {
        return _context.ShopProducts.Where(sp => sp.ShopId == shopId && sp.ApiUrl.Trim() == itemUrl.Trim()).FirstOrDefaultAsync(cancellationToken);
    }

    public Task<ShopProduct?> GetShopProductByShopAndItemId(int shopId, string itemId, CancellationToken cancellationToken = default)
    {
        return _context.ShopProducts.Where(sp => sp.ShopId == shopId && sp.ItemId.Trim() == itemId.Trim()).FirstOrDefaultAsync(cancellationToken);
    }

    public Task<ShopProduct?> GetShopProductByShopAndProductId(int shopId, long productId, CancellationToken cancellationToken = default)
    {
        return _context.ShopProducts.Where(sp => sp.ShopId == shopId && sp.ProductId == productId).FirstOrDefaultAsync(cancellationToken);
    }
}