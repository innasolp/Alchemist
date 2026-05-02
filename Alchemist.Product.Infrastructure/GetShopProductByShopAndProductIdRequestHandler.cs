using Alchemist.Product.Data;
using Alchemist.Product.Infrastructure.Interfaces;
using MediatR;

namespace Alchemist.Product.Infrastructure;

public class GetShopProductByShopAndProductIdRequestHandler(IShopProductRepository repository) : IRequestHandler<GetShopProductByShopAndProductIdRequest, ShopProduct>
{
    private readonly IShopProductRepository _repository = repository;

    public Task<ShopProduct> Handle(GetShopProductByShopAndProductIdRequest request, CancellationToken cancellationToken = default)
    {
        return _repository.GetShopProductByShopAndProductId(request.ShopId, request.ProductId, cancellationToken)!;
    }
}