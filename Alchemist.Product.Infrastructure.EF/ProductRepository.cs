using Alchemist.Product.Data;
using Alchemist.Product.Infrastructure.Interfaces;
using Mediator.Infrastructure;
using Microsoft.EntityFrameworkCore;
namespace Alchemist.Product.Infrastructure.EF;

public class ProductRepository(AlchemyContext context) : IProductRepository
{
    private readonly AlchemyContext _context = context;

    public async Task<Data.Product?> FindProductByNameAndBrand(string name, string brand, CancellationToken cancellationToken = default)
    {
        //StringComparison not available in EF Core queries
        var brands = await _context.Brands.Where(b => b.Name.Trim().ToUpper() == brand.Trim().ToUpper()).ToListAsync(cancellationToken);
        if (brands.Count == 0) return default;
        var products = await _context.Products.Where(p => p.Name.Trim().ToUpper() == name.Trim().ToUpper()).ToListAsync(cancellationToken);
        products = [.. products.Where(p => brands.Any(b => b.Id == p.BrandId))];
        return products.Count > 1
            ? throw new EntityWarningException($"multiple products with name {name} and brand {brand}", products.FirstOrDefault())
            : products.FirstOrDefault();
    }
}