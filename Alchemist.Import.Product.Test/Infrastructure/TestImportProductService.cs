using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Products.Service;
using Import.Interfaces;
using Import.Service.Test.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Alchemist.Import.Product.Test.Infrastructure;

public class TestImportProductService<TCategory, TProductItem>(ILogger logger,
    string name,
    IProductShopModel shopModel, 
    ILoaderService loader,
    IProductItemHandler itemHandler) 
    : ShopImportCategoryProductsService<TCategory, TProductItem>(logger, shopModel.ProductUrl,
            shopModel.CategoryUrl,
            shopModel.Name,
            shopModel.Url,
            shopModel.Categories, loader, itemHandler)
    where TCategory : class, ICategoryProducts, new()
    where TProductItem : class, IProductItem, new()
{
    private readonly IProductShopModel _productShopModel = shopModel;

    public override string Name { get; } = name;
    private int _pageCount = 1000;

    protected override int PageProductCount => _pageCount;

    public void SetPageProductCount(int pageCount)
    {
        _pageCount = pageCount;
    }

    protected override string GetApiUrl(ICategoryProductItem productItem)
    {
        return string.Format(_productShopModel.ProductUrl, productItem.Name);
    }

    public string GetTestApiUrl(ICategoryProductItem productItem)
    {
        return GetApiUrl(productItem);
    }

    protected override bool IsEndOfCategory(TCategory category)
    {
        return !(category.CategoryProductItems?.Length > 0);
    }
}
