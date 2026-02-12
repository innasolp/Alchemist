using Alchemist.Product.Data;
using Alchemist.Product.Infrastructure.Interfaces;
using MediatR;

namespace Alchemist.Product.Infrastructure;

public class GetShopProductByShopAndItemIdRequestHandler(IShopProductRepository repository) 
    : IRequestHandler<GetShopProductByShopAndItemIdRequest, ShopProduct>
{
    private readonly IShopProductRepository _repository = repository;

    public Task<ShopProduct> Handle(GetShopProductByShopAndItemIdRequest request, CancellationToken cancellationToken = default)
    {
        return _repository.GetShopProductByShopAndItemId(request.ShopId, request.ItemId, cancellationToken)!;
    }
}