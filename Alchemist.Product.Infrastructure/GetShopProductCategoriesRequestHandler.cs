using Alchemist.Product.Data;
using Alchemist.Product.UnitOfWork.Interfaces;
using MediatR;

namespace Alchemist.Product.Infrastructure;

public class GetShopProductCategoriesRequestHandler(IShopProductCategoryRepository repository) : IRequestHandler<GetShopProductCategoriesRequest, List<ShopProductCategory>>
{
    protected IShopProductCategoryRepository Repository { get; } = repository;

    public Task<List<ShopProductCategory>> Handle(GetShopProductCategoriesRequest request, CancellationToken cancellationToken  =default)
    {
       return Repository.GetShopProductCategories(request.ShopId, cancellationToken);
    }
}