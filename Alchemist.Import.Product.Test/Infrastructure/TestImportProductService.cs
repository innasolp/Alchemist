using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Products.Service;
using Import.Interfaces;
using Microsoft.Extensions.Logging;

namespace Alchemist.Import.Product.Test.Infrastructure;

public class TestImportProductService<TCategory, TProductItem>(ILogger logger,
    string name,
    IProductShopModel shopModel, 
    ILoaderService loader,
    IProductItemHandler itemHandler) 
    : ShopImportCategoryProductsService<TCategory, TProductItem>(logger,loader, shopModel.Url,
        shopModel.Categories,  itemHandler,
        shopModel.ProductUrl,
            shopModel.CategoryUrlFormat,
            shopModel.Name)
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

    protected override string GetApiUrl(string productUrlFormat, ICategoryProductItem productItem)
    {
        return string.Format(productUrlFormat, productItem.Name);
    }

    public string GetTestApiUrl(ICategoryProductItem productItem)
    {
        return GetApiUrl(_productShopModel.ProductUrl, productItem);
    }

    protected override bool IsEndOfCategory(TCategory category)
    {
        return !(category.CategoryProductItems?.Length > 0);
    }
}
