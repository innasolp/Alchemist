using Alchemist.Product.Data;
using Alchemist.Product.UnitOfWork.Interfaces;
using UnitOfWork;

namespace Alchemist.Product.UnitOfWork;

public class ProductComponentRepository(AlchemyContext context) : EFRepository<ProductComponent, AlchemyContext>(context), IProductComponentRepository
{
    public async Task<ProductComponent> SetProductComponent(ProductComponent productComponent, CancellationToken cancellationToken = default)
    {
        var existed = await Context.ProductComponents.FindAsync([productComponent.ProductId, productComponent.ComponentId], cancellationToken);

        if (existed == null)
        {
            return await Context.Create(productComponent, cancellationToken);
        }
        else if (existed.SequalNumber != productComponent.SequalNumber)
        {
            existed.SequalNumber = productComponent.SequalNumber;
            Context.Update(existed);
            await Context.SaveChangesAsync(cancellationToken);
            return existed;
        }

        return existed;
    }
}