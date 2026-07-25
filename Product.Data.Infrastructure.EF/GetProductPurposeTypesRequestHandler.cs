using Alchemist.Product.Data;
using Db.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Product.Data.Infrastructure.EF;

public class GetProductPurposeTypesRequestHandler(DbContext dbContext) : IRequestHandler<GetProductPurposeTypesRequest, List<PurposeType>>
{
    public async Task<List<PurposeType>> Handle(GetProductPurposeTypesRequest request, CancellationToken cancellationToken = default)
    {
        var purposeTypes = await dbContext.Set<ProductPurpose>()
            .Where(pp => pp.ProductId == request.ProductId)
            .Join(dbContext.Set<PurposeType>(), pp => pp.PurposeTypeId, pt => pt.Id,
                  (pp, pt) => new PurposeType { Id = pt.Id, Name = pt.Name })
            .ToListAsync(cancellationToken);

        return purposeTypes;
    }
}