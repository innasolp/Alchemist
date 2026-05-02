using Alchemist.Import.Products.Interfaces;

namespace Alchemist.Import.Products.Service;

public abstract class CategoryItemPaging<TCategory> : CategoryPaging<TCategory>
    where TCategory : class, ICategoryProducts, IPagingItem
{
    public override string GetCategoryPagePath(IProductShopCategory productShopCategory, string pathFormat, PathFormatType pathFormatType, int page, TCategory? category = null)
    {
        return category == null
           ? base.GetCategoryPagePath(productShopCategory, pathFormat, pathFormatType, page, category)
           : category.GetPageUrl(pathFormat, $"{productShopCategory.ItemId}", page)
           ?? base.GetCategoryPagePath(productShopCategory, pathFormat, pathFormatType, page, category);
    }

    public override string GetNextCategoryPagePath(IProductShopCategory productShopCategory, string pathFormat, PathFormatType pathFormatType, int page, TCategory? category = null)
    {
        return base.GetNextCategoryPagePath(productShopCategory, pathFormat, pathFormatType, page, category);
    }
}