using Alchemist.Product.Shop.Ozon.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Products.Service;
using Import.Interfaces;
namespace Alchemist.Product.Shop.Ozon.ImportService;

public class OzonImportService(ILogger<OzonImportService> logger,
    [FromKeyedServices(OzonImportServiceConstants.OzonKey)] string name,
    string productUrlFormat,
        string categoryUrlFormat,
        string sourceName,
        string url,
        IEnumerable<IProductShopCategory> shopCategories,
    [FromKeyedServices(OzonImportServiceConstants.OzonKey)] ILoaderService loaderService,
    [FromKeyedServices(OzonImportServiceConstants.OzonKey)] IProductItemHandler itemHandler)
    : ShopImportPaginatorCategoryProductsService<Category, Model.Product>(logger, productUrlFormat, categoryUrlFormat, sourceName, url, shopCategories, loaderService, itemHandler)
{
    public override string Name { get; } = name;

    protected override int PageProductCount => 12;

    protected override string GetApiUrl(string productUrlFormat, ICategoryProductItem productItem)
    {
        var ozonCategoryItem = productItem as ProductItem;
        return ozonCategoryItem != null
            ? string.Format(productUrlFormat, ozonCategoryItem.Name)
            : throw new InvalidCastException("Item is not Ozon");
    }

    protected override bool IsEndOfCategory(Category category)
    {
        return category.CategoryContent == null;
    }
}