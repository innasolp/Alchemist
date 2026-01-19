using Alchemist.Product.Data;
using Shop.UnitOfWork;
using MediatR;

namespace Shop.Infrastructure;

public class GetAllCategoryChildrenRequestHandler(IShopCategoryRepository repository) : IRequestHandler<GetAllCategoryChildrenRequest, List<ShopCategory>>
{
    private readonly IShopCategoryRepository _repository = repository;

    public Task<List<ShopCategory>> Handle(GetAllCategoryChildrenRequest request, CancellationToken cancellationToken = default)
    {
        return _repository.GetAllCategoryChildren(request.ParentId, cancellationToken);
    }
}