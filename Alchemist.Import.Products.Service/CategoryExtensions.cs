using Alchemist.Common;
using Alchemist.Import.Interfaces;
using Alchemist.Import.Products.Interfaces;

namespace Alchemist.Import.Products.Service;

public static class CategoryExtensions
{
    private const string Separator = "/";

    public static string? GetCategoryUrl(this IProductShopCategory shopCategory)
    {
        var category = shopCategory.Category?.Split(Separator).LastOrDefault(c => !string.IsNullOrEmpty(c));
        return !string.IsNullOrEmpty(category) && category.Contains(shopCategory.ItemId.ToString()) ? category : shopCategory.ItemId.ToString();
    }
}

public static class CategoryPaging
{
    public static string? GetNextPage<TCategoryProducts>(TCategoryProducts? category, string urlFormat, string itemId, int page)
            where TCategoryProducts : ICategoryProducts
    {
        return typeof(TCategoryProducts).IsImplementation(typeof(IPaginatorItem))
            ? (category as IPaginatorItem)?.GetNextPageUrl(urlFormat, page)
            : string.Format(urlFormat, itemId, page);
    }
}
