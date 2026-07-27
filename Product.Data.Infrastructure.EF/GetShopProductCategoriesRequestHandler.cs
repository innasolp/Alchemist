using Alchemist.Product.Data;
using Db.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Product.Data.Infrastructure.EF;


public class GetShopProductCategoriesRequestHandler(DbContext dbContext) : IRequestHandler<GetShopProductCategoriesRequest, List<ShopProductCategory>>
{
    public Task<List<ShopProductCategory>> Handle(GetShopProductCategoriesRequest request, CancellationToken cancellationToken  =default)
    {
       return dbContext.Set<ShopProductCategory>().Where(s => s.ShopProductId == request.ShopProductId).ToListAsync(cancellationToken);
    }
}