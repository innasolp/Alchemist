using Alchemist.Product.Data;
using Db.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Product.Data.Infrastructure.EF;

public class CheckShopProductCategoryRequestHandler(DbContext dbContext) : IRequestHandler<CheckShopProductCategoryRequest, bool>
{    
    public Task<bool> Handle(CheckShopProductCategoryRequest request, CancellationToken cancellationToken  = default)
    {
        return dbContext.Set<ShopProductCategory>().AnyAsync(s => s.ShopProductId == request.ShopProductId && s.ShopCategoryId == request.ShopCategoryId,
            cancellationToken);
    }
}