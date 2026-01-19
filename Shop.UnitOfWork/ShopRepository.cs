using Alchemist.Product.Data;
using Microsoft.EntityFrameworkCore;
using UnitOfWork;

namespace Shop.UnitOfWork;

public class ShopRepository(AlchemyContext context) : EFRepository<Alchemist.Product.Data.Shop, AlchemyContext>(context), IShopRepository
{
    public async Task<Alchemist.Product.Data.Shop?> GetShopByUrl(string url, CancellationToken cancellationToken = default)
    {
        var shops = await Context.Shops.Where(s => s.Url.ToUpper() == url.ToUpper()).ToListAsync(cancellationToken);
        if (shops.Count > 1)
            throw new EntityWarningException($"multiple shops with url {url}");
        return shops.FirstOrDefault();
    }
}

public class ShopRepository<T>(AlchemyContext alchemyContext) : EFRepository<T, AlchemyContext>(alchemyContext), IRepository<T>
    where T : class
{
}