using Alchemist.Product.Data;
using Db.Infrastructure;
using Db.Infrastructure.EF;

namespace Product.Data.Infrastructure.EF;

public class SetProductComponentCommandHandler(AlchemyContext context, IUnitOfWork unitOfWork) 
    : TransactionalCommandHandler<SetProductComponentCommand>(unitOfWork)
{
    private readonly AlchemyContext _context = context;

    protected override async Task<ProductComponent> HandleCommand(SetProductComponentCommand request, CancellationToken cancellationToken = default)
    {
        var existed = await _context.ProductComponents.FindAsync([request.Entity.ProductId, request.Entity.ComponentId], cancellationToken);

        if (existed == null)
        {
            return await _context.Create(new ProductComponent { ComponentId = request.Entity.ComponentId , ProductId = request.Entity.ProductId } 
            , cancellationToken);
        }
        else if (existed.SequalNumber != request.Entity.SequalNumber)
        {
            existed.SequalNumber = request.Entity.SequalNumber;
            _context.Update(existed);
            await _context.SaveChangesAsync(cancellationToken);
            return existed;
        }

        return existed;
    }
}