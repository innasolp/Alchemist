using Alchemist.Import.Products.Interfaces;

namespace Alchemist.Import.Products.Service;

public static class CategoryExtensions
{
    private const string Separator = "/";

    public static string? GetCategoryUrlWithId(this IProductShopCategory shopCategory)
    {
        var category = shopCategory.Category?.Split(Separator).LastOrDefault(c => !string.IsNullOrEmpty(c));
        return !string.IsNullOrEmpty(category) && category.Contains(shopCategory.ItemId.ToString()) ? category : shopCategory.ItemId.ToString();
    }
}