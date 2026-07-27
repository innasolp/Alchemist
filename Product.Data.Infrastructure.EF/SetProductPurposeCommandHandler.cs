using Alchemist.Product.Data;
using Db.Infrastructure;
using Db.Infrastructure.EF;
using Microsoft.EntityFrameworkCore;

namespace Product.Data.Infrastructure.EF;

public class SetProductPurposeCommandHandler(AlchemyContext context, IUnitOfWork unitOfWork)
    : TransactionalCommandHandler<SetProductPurposeCommand>(unitOfWork)
{
    private readonly AlchemyContext _context = context;

    protected override async Task<ProductPurpose> HandleCommand(SetProductPurposeCommand request, CancellationToken cancellationToken = default)
    {
        var existed = await _context.ProductPurposes.FirstOrDefaultAsync(
            pp => pp.ProductId == request.Entity.ProductId && pp.PurposeTypeId == request.Entity.PurposeTypeId,
            cancellationToken);

        if (existed == null)
        {
            return await _context.Create(request.Entity, cancellationToken);
        }
        else
        {
            var updated = _context.Update(existed);
            await _context.SaveChangesAsync(cancellationToken);
            return updated.Entity;
        }
    }
}