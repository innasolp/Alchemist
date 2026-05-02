using Alchemist.Product.Data;
using Alchemist.Product.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Alchemist.Product.Infrastructure.EF;

public class ProductPurposeTypeRepository(AlchemyContext context) : IProductPurposeTypeRepository
{
    private readonly AlchemyContext _context = context;

    public async Task<List<PurposeType>> GetProductPurposeTypes(long productId, CancellationToken cancellationToken = default)
    {
        var purposeTypes = await _context.ProductPurposes
            .Where(pp => pp.ProductId == productId)
            .Join(_context.PurposeTypes, pp => pp.PurposeTypeId, pt => pt.Id,
                  (pp, pt) => new PurposeType { Id = pt.Id, Name = pt.Name })
            .ToListAsync(cancellationToken);

        return purposeTypes;
    }
}