using Alchemist.Product.Data;
using UnitOfWork;

namespace Alchemist.Product.UnitOfWork.Interfaces;

public interface IProductComponentRepository : IRepository<ProductComponent>
{
    Task<ProductComponent> SetProductComponent(ProductComponent productComponent, CancellationToken cancellationToken = default);
}