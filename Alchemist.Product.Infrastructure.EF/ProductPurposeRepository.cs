using Alchemist.Product.Data;
using Alchemist.Product.Infrastructure.Interfaces;
using Mediator.Infrastructure.EF;
using Microsoft.EntityFrameworkCore;

namespace Alchemist.Product.Infrastructure.EF;

public class ProductPurposeRepository(AlchemyContext context) : IProductPurposeRepository
{
    private readonly AlchemyContext _context = context;

    public async Task<ProductPurpose> SetProductPurpose(ProductPurpose productPurpose, CancellationToken cancellationToken = default)
    {
        var existed = await _context.ProductPurposes.FirstOrDefaultAsync(
            pp => pp.ProductId == productPurpose.ProductId && pp.PurposeTypeId == productPurpose.PurposeTypeId,
            cancellationToken);

        if (existed == null)
        {
            return await _context.Create(productPurpose, cancellationToken);
        }
        else
        {
            var updated = _context.Update(existed);
            await _context.SaveChangesAsync(cancellationToken);
            return updated.Entity;
        }
    }
}