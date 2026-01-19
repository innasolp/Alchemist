using Alchemist.Product.Data;
using Alchemist.Product.UnitOfWork.Interfaces;
using MediatR;

namespace Alchemist.Product.Infrastructure;

public class GetProductPurposeTypesRequestHandler(IProductPurposeTypeRepository repository) : IRequestHandler<GetProductPurposeTypesRequest, List<PurposeType>>
{
    private readonly IProductPurposeTypeRepository _repository = repository;

    public Task<List<PurposeType>> Handle(GetProductPurposeTypesRequest request, CancellationToken cancellationToken = default)
    {
        return _repository.GetProductPurposeTypes(request.ProductId, cancellationToken);
    }
}