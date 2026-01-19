using Alchemist.Product.Data;

namespace Alchemist.Product.UnitOfWork.Interfaces;

public interface IProductPurposeTypeRepository
{
    Task<List<PurposeType>> GetProductPurposeTypes(long productId, CancellationToken cancellationToken = default);
}
