using Alchemist.Product.Data;
using Alchemist.Product.UnitOfWork.Interfaces;
using Microsoft.EntityFrameworkCore;
using UnitOfWork;

namespace Alchemist.Product.UnitOfWork;

public class ShopProductPriceRepository(AlchemyContext context) : EFRepository<ShopProductPrice, AlchemyContext>(context), IShopProductPriceRepository
{
    public Task<ShopProductPrice?> GetShopProductPrice(long shopProductId, CancellationToken cancellationToken = default)
    {
        return Context.ShopProductPrices.FirstOrDefaultAsync(spp => spp.ShopProductId == shopProductId, cancellationToken);
    }
}