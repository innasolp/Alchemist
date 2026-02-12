using Alchemist.Product.Data;

namespace Alchemist.Product.Infrastructure.Interfaces;

public interface IProductPurposeRepository
{
    Task<ProductPurpose> SetProductPurpose(ProductPurpose productPurpose, CancellationToken cancellationToken = default);
}