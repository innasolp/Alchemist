using Alchemist.Product.Data;

namespace Alchemist.Product.Infrastructure.Interfaces;

public interface IProductComponentRepository
{
    Task<ProductComponent> SetProductComponent(ProductComponent productComponent, CancellationToken cancellationToken = default);
}