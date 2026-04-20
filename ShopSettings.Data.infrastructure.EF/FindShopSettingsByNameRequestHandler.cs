using Db.Infrastructure;
using Db.Infrastructure.Requests;
using Microsoft.EntityFrameworkCore;

namespace ShopSettings.Data.infrastructure.EF;

public class FindShopSettingsByNameRequestHandler(DbContext dbContext) : IRequestHandler<FindByNameRequest<Alchemist.Product.Data.ShopSettings>, Alchemist.Product.Data.ShopSettings?>
{
    public Task<Alchemist.Product.Data.ShopSettings?> Handle(FindByNameRequest<Alchemist.Product.Data.ShopSettings> request, CancellationToken cancellationToken)
    {
        return dbContext.Set<Alchemist.Product.Data.ShopSettings>()
            .Where(e=>e.Name.Trim().ToUpper() == request.Name.Trim().ToUpper())
            .FirstOrDefaultAsync(cancellationToken);
    }
}