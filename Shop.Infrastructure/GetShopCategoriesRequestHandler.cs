using Alchemist.Product.Data;
using Shop.UnitOfWork;
using MediatR;

namespace Shop.Infrastructure;

public class GetShopCategoriesRequestHandler(IShopCategoryRepository repository) : IRequestHandler<GetShopCategoriesRequest, List<ShopCategory>>
{
    private readonly IShopCategoryRepository _repository = repository;

    public Task<List<ShopCategory>> Handle(GetShopCategoriesRequest request, CancellationToken cancellationToken = default)
    {
        return _repository.GetShopCategories(request.ShopId, cancellationToken);
    }
}