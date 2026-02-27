using Alchemist.Import.Products.Interfaces;

namespace Alchemist.Import.Products.Service;

public abstract class CategoryPaging<TCategory> : ICategoryPaging<TCategory>
    where TCategory : class, ICategoryProducts
{
    protected virtual object[] GetCategoryPathArguments(IProductShopCategory productShopCategory, PathFormatType pathFormatType)
    {
        return pathFormatType switch
        {
            PathFormatType.ItemId => [productShopCategory.ItemId],
            PathFormatType.Path => [PreparePath(productShopCategory.Path)],
            PathFormatType.CategoryWithItemId => [PreparePath(productShopCategory.Category), productShopCategory.ItemId],
            _ => [productShopCategory.Path],
        };
    }

    protected abstract string PreparePath(string path);

    public virtual string GetCategoryPagePath(IProductShopCategory productShopCategory, 
        string pathFormat, 
        PathFormatType pathFormatType,
        int page,
        TCategory? category = null)
    {
        List<object> args = [.. GetCategoryPathArguments(productShopCategory, pathFormatType), page];

        return string.Format(pathFormat, args: [.. args]);
    }

    public virtual string GetNextCategoryPagePath(IProductShopCategory productShopCategory, 
        string pathFormat,
        PathFormatType pathFormatType,
        int page,
        TCategory? category = null)
    {
        return GetCategoryPagePath(productShopCategory, pathFormat, pathFormatType, page + 1, category);
    }
}