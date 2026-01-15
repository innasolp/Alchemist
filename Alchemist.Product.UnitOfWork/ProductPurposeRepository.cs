using Alchemist.Product.Data;
using Alchemist.Product.UnitOfWork.Interfaces;
using Microsoft.EntityFrameworkCore;
using UnitOfWork;

namespace Alchemist.Product.UnitOfWork;

public class ProductPurposeRepository(AlchemyContext context) : Repository<ProductPurpose, AlchemyContext>(context), IProductPurposeRepository
{
    public async Task<ProductPurpose> SetProductPurpose(ProductPurpose productPurpose, CancellationToken cancellationToken = default)
    {
        var existed = await Context.ProductPurposes.FirstOrDefaultAsync(
            pp => pp.ProductId == productPurpose.ProductId && pp.PurposeTypeId == productPurpose.PurposeTypeId,
            cancellationToken);

        if (existed == null)
        {
            return await Context.Create(productPurpose, cancellationToken);
        }
        else
        {
            var updated = Context.Update(existed);
            await Context.SaveChangesAsync(cancellationToken);
            return updated.Entity;
        }
    }
}