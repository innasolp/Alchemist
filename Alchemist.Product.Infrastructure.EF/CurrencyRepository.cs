using Alchemist.Product.Data;
using Alchemist.Product.Infrastructure.Interfaces;
using Mediator.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Alchemist.Product.Infrastructure.EF;

public class CurrencyRepository(AlchemyContext context) : ICurrencyRepository
{
    private readonly AlchemyContext _context = context;

    public async Task<Currency?> GetCurrencyByCode(short code, CancellationToken cancellationToken = default)
    {
        var entities = await _context.Currencies.Where(e => e.Code == code).ToListAsync(cancellationToken);
        return entities.Count > 1
            ? throw new EntityWarningException($"multiple currencies with code {code}", entities.FirstOrDefault())
            : entities.FirstOrDefault();
    }
}