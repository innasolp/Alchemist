using Alchemist.Product.Data;
using UnitOfWork;

namespace Alchemist.Product.UnitOfWork.Interfaces;

public interface ICurrencyRepository : IRepository<Currency>
{
    Task<Currency?> GetCurrencyByCode(short code, CancellationToken cancellationToken = default);
}
