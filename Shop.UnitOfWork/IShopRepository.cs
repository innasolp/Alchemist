using UnitOfWork;

namespace Shop.UnitOfWork;

public interface IShopRepository : IRepository<Alchemist.Product.Data.Shop>
{
    Task<Alchemist.Product.Data.Shop?> GetShopByUrl(string url, CancellationToken cancellationToken = default);
}
