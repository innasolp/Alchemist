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
    object? productLoadData = null,
    object? categoryLoadData = null,
    UrlFormatType productUrlFormatType = default,
    UrlFormatType categoryUrlFormatType = default) 
    : ShopImportCategoryProductsService<TCategory, TProductItem>(logger,
        loader,
        url,
        shopCategories,
        itemHandler,
        productUrlFormat,
        categoryUrlFormat,
        sourceName,
        productLoadData,
        categoryLoadData,
        productUrlFormatType,
        categoryUrlFormatType)
    where TCategory : class, ICategoryProducts, IPaginatorItem, new()
    where TProductItem : class, IProductItem, new()
{
    protected override string GetCategoryPageUrl(IProductShopCategory productShopCategory, string urlFormat, UrlFormatType urlFormatType, int page, TCategory? category = null)
    {
        return category == null
           ? base.GetCategoryPageUrl(productShopCategory, urlFormat, urlFormatType, page, category)
           : category.GetPageUrl(urlFormat, $"{productShopCategory.ItemId}", page);
    }

    protected override string GetNextCategoryPageUrl(IProductShopCategory productShopCategory, string urlFormat, UrlFormatType urlFormatType, int page, TCategory? category = null)
    {
        return category == null
           ? base.GetNextCategoryPageUrl(productShopCategory, urlFormat, urlFormatType, page, category)
           : category.GetNextPageUrl(urlFormat, $"{productShopCategory.ItemId}", page);
    }
}