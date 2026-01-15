using Alchemist.Product.Data;
using Alchemist.Product.UnitOfWork.Interfaces;
using Microsoft.EntityFrameworkCore;
using UnitOfWork;

namespace Alchemist.Product.UnitOfWork;

public class ProductPurposeTypeRepository(AlchemyContext context) : EFRepository<PurposeType, AlchemyContext>(context), IProductPurposeTypeRepository
{
    public async Task<List<PurposeType>> GetProductPurposeTypes(long productId, CancellationToken cancellationToken = default)
    {
        var purposeTypes = await Context.ProductPurposes
            .Where(pp => pp.ProductId == productId)
            .Join(Context.PurposeTypes, pp => pp.PurposeTypeId, pt => pt.Id,
                  (pp, pt) => new PurposeType { Id = pt.Id, Name = pt.Name })
            .ToListAsync(cancellationToken);

        return purposeTypes;
    }

}