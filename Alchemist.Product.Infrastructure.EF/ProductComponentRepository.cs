using Alchemist.Product.Data;
using Alchemist.Product.Infrastructure.Interfaces;
using Mediator.Infrastructure.EF;

namespace Alchemist.Product.Infrastructure.EF;

public class ProductComponentRepository(AlchemyContext context) : IProductComponentRepository
{
    private readonly AlchemyContext _context = context;

    public async Task<ProductComponent> SetProductComponent(ProductComponent productComponent, CancellationToken cancellationToken = default)
    {
        var existed = await _context.ProductComponents.FindAsync([productComponent.ProductId, productComponent.ComponentId], cancellationToken);

        if (existed == null)
        {
            return await _context.Create(productComponent, cancellationToken);
        }
        else if (existed.SequalNumber != productComponent.SequalNumber)
        {
            existed.SequalNumber = productComponent.SequalNumber;
            _context.Update(existed);
            await _context.SaveChangesAsync(cancellationToken);
            return existed;
        }

        return existed;
    }
}