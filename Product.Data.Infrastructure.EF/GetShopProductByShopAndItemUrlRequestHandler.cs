using Alchemist.Product.Data;
using Db.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Product.Data.Infrastructure.EF;

public class GetShopProductByShopAndItemUrlRequestHandler(DbContext dbContext) : IRequestHandler<GetShopProductByShopAndItemUrlRequest, ShopProduct?>
{
    public Task<ShopProduct?> Handle(GetShopProductByShopAndItemUrlRequest request, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<ShopProduct>().Where(sp => sp.ShopId == request.ShopId && sp.ApiUrl.Trim() == request.ItemUrl.Trim())
            .FirstOrDefaultAsync(cancellationToken);
    }
}