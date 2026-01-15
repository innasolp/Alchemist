using Alchemist.Product.Data;
using Alchemist.Product.UnitOfWork.Interfaces;
using Microsoft.EntityFrameworkCore;
using UnitOfWork;

namespace Alchemist.Product.UnitOfWork;

public class ProductRepository(AlchemyContext context) : EFRepository<Data.Product, AlchemyContext>(context), IProductRepository
{
    public async Task<Data.Product?> FindProductByNameAndBrand(string name, string brand, CancellationToken cancellationToken = default)
    {
        //StringComparison not available in EF Core queries
        var brands = await Context.Brands.Where(b => b.Name.Trim().ToUpper() == brand.Trim().ToUpper()).ToListAsync(cancellationToken);
        if (brands.Count == 0) return default;
        var products = await Context.Products.Where(p => p.Name.Trim().ToUpper() == name.Trim().ToUpper()).ToListAsync(cancellationToken);
        products = [.. products.Where(p => brands.Any(b => b.Id == p.BrandId))];
        return products.Count > 1
            ? throw new EntityWarningException($"multiple products with name {name} and brand {brand}", products.FirstOrDefault())
            : products.FirstOrDefault();
    }
}