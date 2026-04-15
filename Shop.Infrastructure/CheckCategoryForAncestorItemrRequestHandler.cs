using MediatR;

namespace Shop.Infrastructure;

internal class CheckCategoryForAncestorItemrRequestHandler(IShopCategoryRepository repository) 
    : IRequestHandler<CheckCategoryForAncestorItemrRequest, bool?>
{
    private readonly IShopCategoryRepository _repository = repository;

    public Task<bool?> Handle(CheckCategoryForAncestorItemrRequest request, CancellationToken cancellationToken)
    {
        return _repository.CheckCategoryForAncestorItem(request.Id, request.AncestorItemId, cancellationToken);
    }
}