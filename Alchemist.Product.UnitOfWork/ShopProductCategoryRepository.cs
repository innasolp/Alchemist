using Alchemist.Product.Data;
using Alchemist.Product.Interfaces;
using Alchemist.Product.UnitOfWork.Interfaces;
using Microsoft.EntityFrameworkCore;
using UnitOfWork;

namespace Alchemist.Product.UnitOfWork;

public class ShopProductCategoryRepository(AlchemyContext context) : EFRepository<ShopProductCategory, AlchemyContext>(context), IShopProductCategoryRepository
{
    public async Task<IShopProductCategory> AddShopProductCategory(long shopProductId, int shopCategoryId, CancellationToken cancellationToken = default)
    {
        var shopProductCategoryEntity = new ShopProductCategory { ShopProductId = shopProductId, ShopCategoryId = shopCategoryId };
        return await Context.Create(shopProductCategoryEntity, cancellationToken);
    }

    public Task<bool> CheckShopProductCategory(long shopProductId, int shopCategoryId, CancellationToken cancellationToken = default)
    {
        return Context.ShopProductCategories.AnyAsync(s => s.ShopProductId == shopProductId && s.ShopCategoryId == shopCategoryId, cancellationToken);
    }

    public Task<List<ShopProductCategory>> GetShopProductCategories(long shopProductId, CancellationToken cancellationToken = default)
    {
        return Context.ShopProductCategories.Where(s => s.ShopProductId == shopProductId).ToListAsync(cancellationToken);       
    }
}