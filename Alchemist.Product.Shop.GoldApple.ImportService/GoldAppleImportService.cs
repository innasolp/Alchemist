using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Alchemist.Product.Shop.GoldApple.Model;
using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Products.Service;
using Import.Interfaces;

namespace Alchemist.Product.Shop.GoldApple.ImportService;

public class GoldAppleImportService(ILogger<GoldAppleImportService> logger,
    [FromKeyedServices(GoldAppleConstants.GolAppleKey)] string name,
    [FromKeyedServices(GoldAppleConstants.GolAppleKey)] string productUrlFormat,
    string categoryUrlFormat,
    string sourceName,
    string url,
    IEnumerable<IProductShopCategory> shopCategories,
    [FromKeyedServices(GoldAppleConstants.GolAppleKey)] ILoaderService loader,
    [FromKeyedServices(GoldAppleConstants.GolAppleKey)] IProductItemHandler productDataHandler)
    : ShopImportCategoryProductsService<CategoryProducts, ProductData>(logger, productUrlFormat, categoryUrlFormat, sourceName, url, shopCategories, loader, productDataHandler)
{
    public override string Name { get; } = name;

    protected override int PageProductCount => 24;

    protected override bool IsEndOfCategory(CategoryProducts category)
    {
        return !(category.Data.Products?.Length > 0) ;
    }

    protected override string GetApiUrl(string productUrlFormat, ICategoryProductItem productItem)
    {
        return string.Format(productUrlFormat, productItem.Id);
    }
}
