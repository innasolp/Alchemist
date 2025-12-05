using Alchemist.Import.Products.Interfaces;
using Import.Interfaces;
using Microsoft.Extensions.Logging;

namespace Alchemist.Import.Products.Service;

public abstract class ShopImportPaginatorCategoryProductsService<TCategory, TProductItem>
    (ILogger logger, string productUrlFormat, string categoryUrlFormat, string sourceName, string url, IEnumerable<IProductShopCategory> shopCategories, ILoaderService loader, IProductItemHandler itemHandler) 
    : ShopImportCategoryProductsService<TCategory, TProductItem>(logger, productUrlFormat, categoryUrlFormat, sourceName, url, shopCategories, loader, itemHandler)
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