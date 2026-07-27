using Alchemist.Product.Data;
using Db.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Product.Data.Infrastructure.EF;

public class GetShopProductByShopAndItemIdRequestHandler(DbContext dbContext) 
    : IRequestHandler<GetShopProductByShopAndItemIdRequest, ShopProduct?>
{
    public Task<ShopProduct?> Handle(GetShopProductByShopAndItemIdRequest request, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<ShopProduct>().Where(sp => sp.ShopId == request.ShopId && sp.ItemId.Trim() == request.ItemId.Trim())
            .FirstOrDefaultAsync(cancellationToken);
    }
}