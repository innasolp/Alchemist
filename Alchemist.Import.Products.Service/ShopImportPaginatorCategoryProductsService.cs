using Alchemist.Import.Products.Interfaces;
using Import.Interfaces;
using Microsoft.Extensions.Logging;

namespace Alchemist.Import.Products.Service;

public abstract class ShopImportPaginatorCategoryProductsService<TCategory, TProductItem>
    (ILogger logger,
    ILoaderService loader,
    string url,    
    IEnumerable<IProductShopCategory> shopCategories,    
    IProductItemHandler itemHandler ,
     string productUrlFormat,
     string categoryUrlFormat,
    string sourceName, 
    string? productHttpMethod="GET",
    string? categoryHttpMethod="GET") 
    : ShopImportCategoryProductsService<TCategory, TProductItem>(logger,
        loader,
        url,
        shopCategories,
        itemHandler,
        productUrlFormat,
        categoryUrlFormat,
        sourceName,
        productHttpMethod,
        categoryHttpMethod)
    where TCategory : class, ICategoryProducts, IPaginatorItem, new()
    where TProductItem : class, IProductItem, new()
{
    protected override string GetCategoryPageUrl(string urlFormat, string itemId, int page, TCategory? category = null)
    {
        return category == null 
            ? base.GetCategoryPageUrl(urlFormat, itemId, page, category)
            : category.GetPageUrl(CategoryUrlFormat, itemId, page);
    }

    protected override string GetNextCategoryPageUrl(string urlFormat, string itemId, int page, TCategory? category = null)
    {
        return category == null
            ? base.GetCategoryPageUrl(urlFormat, itemId, page, category)
            : category.GetNextPageUrl(CategoryUrlFormat, itemId, page);
    }
}