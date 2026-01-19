using Alchemist.Product.Data;

namespace Alchemist.Product.UnitOfWork.Interfaces;

public interface IProductPurposeRepository
{
    Task<ProductPurpose> SetProductPurpose(ProductPurpose productPurpose, CancellationToken cancellationToken = default);
}