using Alchemist.Product.Data;
using Db.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Product.Data.Infrastructure.EF;

public class FindProductByNameAndBrandRequestHandler(DbContext dbContext) : IRequestHandler<FindProductByNameAndBrandRequest, Alchemist.Product.Data.Product?>
{
    public async Task<Alchemist.Product.Data.Product?> Handle(FindProductByNameAndBrandRequest request, CancellationToken cancellationToken = default)
    {
        //StringComparison not available in EF Core queries
        var brands = await dbContext.Set<Brand>().Where(b => b.Name.Trim().ToUpper() == request.Brand.Trim().ToUpper()).ToListAsync(cancellationToken);
        if (brands.Count == 0) return default;
        var products = await dbContext.Set<Alchemist.Product.Data.Product>().Where(p => p.Name.Trim().ToUpper() == request.Name.Trim().ToUpper())
            .ToListAsync(cancellationToken);
        products = [.. products.Where(p => brands.Any(b => b.Id == p.BrandId))];
        return products.Count > 1
            ? throw new EntityWarningException($"multiple products with name {request.Name} and brand {request.Brand}", products.FirstOrDefault())
            : products.FirstOrDefault();
    }
}