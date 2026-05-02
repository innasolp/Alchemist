using Alchemist.Product.Data;
using MediatR;

namespace Shop.Infrastructure;

public class GetShopCategoryByShopIdAndItemIdRequestHandler(IShopCategoryRepository repository) 
    : IRequestHandler<GetShopCategoryByShopIdAndItemIdRequest, ShopCategory>
{
    private readonly IShopCategoryRepository _repository = repository;

    public Task<ShopCategory> Handle(GetShopCategoryByShopIdAndItemIdRequest request, CancellationToken cancellationToken = default)
    {
        return _repository.GetShopCategoryByShopIdAndItemId(request.ShopId, request.ItemId, cancellationToken)!;
    }
}