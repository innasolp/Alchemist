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
    ImportProductServiceOptions importProductServiceOptions) 
    : ShopImportCategoryProductsService<TCategory, TProductItem>(logger,
        loader,
        url,
        shopCategories,
        itemHandler,
        productUrlFormat,
        categoryUrlFormat,
        sourceName, importProductServiceOptions)
    where TCategory : class, ICategoryProducts, IPaginatorItem, new()
    where TProductItem : class, IProductItem, new()
{
    protected override string GetCategoryPagePath(IProductShopCategory productShopCategory, string urlFormat, PathFormatType urlFormatType, int page, TCategory? category = null)
    {
        return category == null
           ? base.GetCategoryPagePath(productShopCategory, urlFormat, urlFormatType, page, category)
           : category.GetPageUrl(urlFormat, $"{productShopCategory.ItemId}", page)
           ?? base.GetCategoryPagePath(productShopCategory, urlFormat, urlFormatType, page, category);
    }

    protected override string GetNextCategoryPagePath(IProductShopCategory productShopCategory, string urlFormat, PathFormatType urlFormatType, int page, TCategory? category = null)
    {
        return category == null
           ? base.GetNextCategoryPagePath(productShopCategory, urlFormat, urlFormatType, page, category)
           : category.GetNextPageUrl(urlFormat, $"{productShopCategory.ItemId}", page)
           ?? category.GetNextPageUrl(urlFormat, $"{productShopCategory.ItemId}", page); ;
    }
}