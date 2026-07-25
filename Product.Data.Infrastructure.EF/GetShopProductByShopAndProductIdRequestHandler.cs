using Alchemist.Product.Data;
using Db.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Product.Data.Infrastructure.EF;

public class GetShopProductByShopAndProductIdRequestHandler(DbContext dbContext) : IRequestHandler<GetShopProductByShopAndProductIdRequest, ShopProduct?>
{
    public Task<ShopProduct?> Handle(GetShopProductByShopAndProductIdRequest request, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<ShopProduct>().Where(sp => sp.ShopId == request.ShopId && sp.ProductId == request.ProductId)
            .FirstOrDefaultAsync(cancellationToken);
    }
}