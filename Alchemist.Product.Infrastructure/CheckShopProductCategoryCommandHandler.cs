using Alchemist.Product.Infrastructure.Interfaces;
using MediatR;

namespace Alchemist.Product.Infrastructure;

public class CheckShopProductCategoryRequestHandler(IShopProductCategoryRepository repository) : IRequestHandler<CheckShopProductCategoryRequest, bool>
{
    protected IShopProductCategoryRepository Repository { get; } = repository;

    public Task<bool> Handle(CheckShopProductCategoryRequest request, CancellationToken cancellationToken  = default)
    {
        return Repository.CheckShopProductCategory(request.ShopProductId, request.ShopCategoryId, cancellationToken);
    }
}