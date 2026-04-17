using Alchemist.Product.Data;
using Db.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Shop.Data.Infrastructure.EF;

public class GetShopCategoryByShopIdAndItemIdRequestHandler(AlchemyContext context) : IRequestHandler<GetShopCategoryByShopIdAndItemIdRequest, ShopCategory>
{
    private AlchemyContext Context { get; } = context;

    public Task<ShopCategory> Handle(GetShopCategoryByShopIdAndItemIdRequest request, CancellationToken cancellationToken = default)
    {
        return Context.ShopCategories.FirstOrDefaultAsync(sc => sc.ShopId == request.ShopId && sc.ItemId == request.ItemId, cancellationToken)!;
    }
}