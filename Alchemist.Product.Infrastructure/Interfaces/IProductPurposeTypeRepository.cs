using Alchemist.Product.Data;

namespace Alchemist.Product.Infrastructure.Interfaces;

public interface IProductPurposeTypeRepository
{
    Task<List<PurposeType>> GetProductPurposeTypes(long productId, CancellationToken cancellationToken = default);
}
