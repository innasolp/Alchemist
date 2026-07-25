using Alchemist.Product.Data;
using Db.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Product.Data.Infrastructure.EF;

public class GetCurrencyByCodeRequestHandler(DbContext dbContext) : IRequestHandler<GetCurrencyByCodeRequest, Currency?>
{
    public async Task<Currency?> Handle(GetCurrencyByCodeRequest request, CancellationToken cancellationToken = default)
    {
        var entities = await dbContext.Set<Currency>().Where(e => e.Code == request.Code).ToListAsync(cancellationToken);
        return entities.Count > 1
            ? throw new EntityWarningException($"multiple currencies with code {request.Code}", entities.FirstOrDefault())
            : entities.FirstOrDefault();
    }
}