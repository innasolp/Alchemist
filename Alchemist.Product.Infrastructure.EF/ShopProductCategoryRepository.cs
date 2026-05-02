using Alchemist.Product.Data;
using Alchemist.Product.Infrastructure.Interfaces;
using Mediator.Infrastructure.EF;
using Microsoft.EntityFrameworkCore;

namespace Alchemist.Product.Infrastructure.EF;

public class ShopProductCategoryRepository(AlchemyContext context) : IShopProductCategoryRepository
{
    private readonly AlchemyContext _context = context;

    public async Task<ShopProductCategory> AddShopProductCategory(long shopProductId, int shopCategoryId, CancellationToken cancellationToken = default)
    {
        var shopProductCategoryEntity = new ShopProductCategory { ShopProductId = shopProductId, ShopCategoryId = shopCategoryId };
        return await _context.Create(shopProductCategoryEntity, cancellationToken);
    }

    public Task<bool> CheckShopProductCategory(long shopProductId, int shopCategoryId, CancellationToken cancellationToken = default)
    {
        return _context.ShopProductCategories.AnyAsync(s => s.ShopProductId == shopProductId && s.ShopCategoryId == shopCategoryId, cancellationToken);
    }

    public Task<List<ShopProductCategory>> GetShopProductCategories(long shopProductId, CancellationToken cancellationToken = default)
    {
        return _context.ShopProductCategories.Where(s => s.ShopProductId == shopProductId).ToListAsync(cancellationToken);       
    }
}