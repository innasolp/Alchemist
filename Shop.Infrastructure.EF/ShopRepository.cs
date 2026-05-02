using Alchemist.Product.Data;
using Mediator.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Shop.Infrastructure.EF;

public class ShopRepository(AlchemyContext context) : IShopRepository
{
    private AlchemyContext Context { get; } = context;

    public async Task<Alchemist.Product.Data.Shop?> GetShopByUrl(string url, CancellationToken cancellationToken = default)
    {
        var shops = await Context.Shops.Where(s => s.Url.ToUpper() == url.ToUpper()).ToListAsync(cancellationToken);
        if (shops.Count > 1)
            throw new EntityWarningException($"multiple shops with url {url}");
        return shops.FirstOrDefault();
    }
}