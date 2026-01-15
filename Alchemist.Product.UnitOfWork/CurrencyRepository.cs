using Alchemist.Product.Data;
using Alchemist.Product.UnitOfWork.Interfaces;
using Microsoft.EntityFrameworkCore;
using UnitOfWork;

namespace Alchemist.Product.UnitOfWork;

public class CurrencyRepository(AlchemyContext context) : EFRepository<Currency, AlchemyContext>(context), ICurrencyRepository
{
    public async Task<Currency?> GetCurrencyByCode(short code, CancellationToken cancellationToken = default)
    {
        var entities = await Context.Currencies.Where(e => e.Code == code).ToListAsync(cancellationToken);
        return entities.Count > 1
            ? throw new EntityWarningException($"multiple currencies with code {code}", entities.FirstOrDefault())
            : entities.FirstOrDefault();
    }
}