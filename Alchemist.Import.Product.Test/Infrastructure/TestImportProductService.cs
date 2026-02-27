using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Products.Service;
using Import.Interfaces;
using Microsoft.Extensions.Logging;

namespace Alchemist.Import.ProductService.Test.Infrastructure;

public class TestImportProductService<TCategory, TProductItem>(ILogger logger,
    string name,
    IProductShopModel shopModel, 
    ILoaderService loader,
    IProductItemHandler itemHandler
    , int pageProductCount = 1000) 
    : ShopImportCategoryProductsService<TCategory, TProductItem>(logger,loader, shopModel.Url,
        shopModel.Categories,  itemHandler,
        shopModel.ProductUrl,
            shopModel.CategoryUrlFormat,
            shopModel.Name,
            new TestCategoryPaging<TCategory>(),
            new ImportProductServiceOptions { PageProductCount = pageProductCount })
    where TCategory : class, ICategoryProducts, new()
    where TProductItem : class, IProductItem, new()
{
    private readonly IProductShopModel _productShopModel = shopModel;

    public override string Name { get; } = name;    

    protected override string GetProductAbsolutePath(string productUrlFormat, ICategoryProductItem productItem)
    {
        return string.Format(productUrlFormat, productItem.Name);
    }

    public string GetTestApiUrl(ICategoryProductItem productItem)
    {
        return GetProductAbsolutePath(_productShopModel.ProductUrl, productItem);
    }

    protected override bool? IsEndOfCategory(TCategory category, int processProductCount)
    {
        return !(category.CategoryProductItems?.Length > 0);
    }

    protected override string PreparePath(string path) => path;
}
