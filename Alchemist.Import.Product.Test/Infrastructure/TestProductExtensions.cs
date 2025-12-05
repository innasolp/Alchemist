using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Products.Service;

namespace Alchemist.Import.Product.Test.Infrastructure;

internal static class TestProductExtensions
{
    public static string GetCategoryPageUrl(this IProductShopModel productShopModel, IProductShopCategory category, int page)
    {
        return string.Format(productShopModel.CategoryUrlFormat, category.GetCategoryUrl(), page);
    }    
}