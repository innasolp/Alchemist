using Alchemist.Product.Data;

namespace Alchemist.Product.Infrastructure.Interfaces;

public interface ICurrencyRepository
{
    Task<Currency?> GetCurrencyByCode(short code, CancellationToken cancellationToken = default);
}