using Alchemist.Product.Data;
using Db.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Shop.Data.Infrastructure.EF;

public class GetShopByUrlRequestHandler(AlchemyContext context) : IRequestHandler<GetShopByUrlRequest, Alchemist.Product.Data.Shop>
{
    private AlchemyContext Context { get; } = context;

    public Task<Alchemist.Product.Data.Shop> Handle(GetShopByUrlRequest request, CancellationToken cancellationToken = default)
    {
        return Context.Shops.FirstOrDefaultAsync(s => s.Url == request.Url, cancellationToken)!;
    }
}