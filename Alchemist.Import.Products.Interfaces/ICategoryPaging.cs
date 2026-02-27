namespace Alchemist.Import.Products.Interfaces;

public interface ICategoryPaging<TCategory>
    where TCategory : class, ICategoryProducts
{
    string GetCategoryPagePath(IProductShopCategory productShopCategory, string pathFormat, PathFormatType pathFormatType, int page, TCategory? category = null);

    string GetNextCategoryPagePath(IProductShopCategory productShopCategory, string urlFormat, PathFormatType urlFormatType, int page, TCategory? category = null);
}