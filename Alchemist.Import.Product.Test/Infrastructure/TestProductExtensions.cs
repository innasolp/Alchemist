using Alchemist.Import.Products.Interfaces;

namespace Alchemist.Import.ProductService.Test.Infrastructure;

internal static class TestProductExtensions
{
    public static string GetCategoryPageUrl(this IProductShopModel productShopModel, IProductShopCategory category, int page)
    {
        return string.Format(productShopModel.CategoryUrlFormat, category.Category, page);
    }    
}