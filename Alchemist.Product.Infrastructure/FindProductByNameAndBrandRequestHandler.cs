using Alchemist.Product.Infrastructure.Interfaces;
using MediatR;

namespace Alchemist.Product.Infrastructure;

public class FindProductByNameAndBrandRequestHandler(IProductRepository repository) : IRequestHandler<FindProductByNameAndBrandRequest, Data.Product>
{
    private readonly IProductRepository _repository = repository;

    public Task<Data.Product> Handle(FindProductByNameAndBrandRequest request, CancellationToken cancellationToken = default)
    {
        return _repository.FindProductByNameAndBrand(request.Name, request.Brand, cancellationToken)!;
    }
}