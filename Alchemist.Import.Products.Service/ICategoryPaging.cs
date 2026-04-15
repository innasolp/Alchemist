using Alchemist.Import.Products.Interfaces;

namespace Alchemist.Import.Products.Service;

public interface ICategoryPaging<TCategory>
    where TCategory : class, ICategoryProducts
{
    string GetCategoryPagePath(IProductShopCategory productShopCategory, string pathFormat, PathFormatType pathFormatType, int page, TCategory? category = null);

    string GetNextCategoryPagePath(IProductShopCategory productShopCategory, string urlFormat, PathFormatType urlFormatType, int page, TCategory? category = null);
}