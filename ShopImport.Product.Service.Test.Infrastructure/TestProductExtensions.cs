using Alchemist.Import.Products.Interfaces;

namespace ShopImport.Product.Service.Test.Infrastructure;

public static class TestProductExtensions
{
    public static string GetCategoryPageUrl(this IProductShopModel productShopModel, IProductShopCategory category, int page)
    {
        return string.Format(productShopModel.CategoryUrlFormat, category.Path, page);
    }    
}