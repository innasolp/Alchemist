using Alchemist.Product.Data;
using Alchemist.Product.Infrastructure.Interfaces;
using MediatR;

namespace Alchemist.Product.Infrastructure;

public class GetShopProductByShopAndItemUrlRequestHandler(IShopProductRepository repository) : IRequestHandler<GetShopProductByShopAndItemUrlRequest, ShopProduct>
{
    private readonly IShopProductRepository _repository = repository;

    public Task<ShopProduct> Handle(GetShopProductByShopAndItemUrlRequest request, CancellationToken cancellationToken = default)
    {
        return _repository.GetShopProductByShopAndItemUrl(request.ShopId, request.ItemUrl, cancellationToken)!;
    }
}