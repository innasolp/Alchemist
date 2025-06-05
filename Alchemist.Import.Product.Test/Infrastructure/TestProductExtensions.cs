using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Products.Service;

namespace Alchemist.Import.Product.Test.Infrastructure;

internal static class TestProductExtensions
{
    public static string GetCategoryPageUrl(this IProductShopModel productShopModel, IProductShopCategoryModel category, int page)
    {
        return string.Format(productShopModel.CategoryUrl, category.GetCategoryForUrl(), page);
    }    
}
