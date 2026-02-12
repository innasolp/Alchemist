using Alchemist.Product.Data;
using Alchemist.Product.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Alchemist.Product.Infrastructure.EF;

public class ShopProductPriceRepository(AlchemyContext context) : IShopProductPriceRepository
{
    private readonly AlchemyContext _context = context;

    public Task<ShopProductPrice?> GetShopProductPrice(long shopProductId, CancellationToken cancellationToken = default)
    {
        return _context.ShopProductPrices.FirstOrDefaultAsync(spp => spp.ShopProductId == shopProductId, cancellationToken);
    }
}