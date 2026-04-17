using Alchemist.Product.Data;
using Db.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Shop.Data.Infrastructure.EF;

public class GetShopCategoriesRequestHandler(AlchemyContext context) : IRequestHandler<GetShopCategoriesRequest, List<ShopCategory>>
{
    private AlchemyContext Context { get; } = context;

    public Task<List<ShopCategory>> Handle(GetShopCategoriesRequest request, CancellationToken cancellationToken = default)
    {
        return Context.ShopCategories.Where(su => su.ShopId == request.ShopId).ToListAsync(cancellationToken);        
    }
}