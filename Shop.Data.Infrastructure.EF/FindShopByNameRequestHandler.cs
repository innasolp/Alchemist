using Db.Infrastructure;
using Db.Infrastructure.Requests;
using Microsoft.EntityFrameworkCore;

namespace Shop.Data.Infrastructure.EF;

public class FindShopByNameRequestHandler(DbContext dbContext) : IRequestHandler<FindByNameRequest<Alchemist.Product.Data.Shop>, Alchemist.Product.Data.Shop?>
{
    public Task<Alchemist.Product.Data.Shop?> Handle(FindByNameRequest<Alchemist.Product.Data.Shop> request, CancellationToken cancellationToken)
    {
        return dbContext.Set<Alchemist.Product.Data.Shop>()
            .Where(e=>e.Name.Trim().ToUpper() == request.Name.Trim().ToUpper())
            .FirstOrDefaultAsync(cancellationToken);
    }
}