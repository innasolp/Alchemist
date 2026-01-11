using Alchemist.Import.Products.Interfaces;
using Import.Interfaces;
using Json.CustomSerialization;
using Microsoft.Extensions.Logging;

namespace Alchemist.Import.Product.Json.Service;

public abstract class ShopImportJsonPaginatorCategoryProductsService<TPagingCategory, TProduct>(ILogger logger,
    ILoaderService loader, 
    string url,
    string serviceName,
    IEnumerable<IProductShopCategory> shopCategories, 
    IProductItemHandler itemHandler, 
    string productUrlFormat, 
    string categoryUrlFormat, 
    string sourceName,
    ImportProductJsonServiceOptions importProductJsonServiceOptions)
    : ShopImportJsonCategoryProductsService<TPagingCategory, TProduct>(logger,
        loader, 
        url,
        serviceName, 
        shopCategories, 
        itemHandler,
        productUrlFormat, 
        categoryUrlFormat, 
        sourceName, importProductJsonServiceOptions)
    where TPagingCategory : class, IPaginatorItem, IJsonItem, ICategoryProducts, new()
    where TProduct : class, IProductItem, new()
{
    protected override string GetCategoryPagePath(IProductShopCategory productShopCategory, string urlFormat, PathFormatType urlFormatType, int page, TPagingCategory? category = null)
    {
        return category == null
           ? base.GetCategoryPagePath(productShopCategory, urlFormat, urlFormatType, page, category)
           : category.GetPageUrl(urlFormat, $"{productShopCategory.ItemId}", page)
              ?? base.GetCategoryPagePath(productShopCategory, urlFormat, urlFormatType, page, category);
    }

    protected override string GetNextCategoryPagePath(IProductShopCategory productShopCategory, string urlFormat, PathFormatType urlFormatType, int page, TPagingCategory? category = null)
    {
        return category == null
           ? base.GetNextCategoryPagePath(productShopCategory, urlFormat, urlFormatType, page, category)
           : category?.GetNextPageUrl(urlFormat, $"{productShopCategory.ItemId}", page)
              ?? base.GetNextCategoryPagePath(productShopCategory, urlFormat, urlFormatType, page, category);
    }
}