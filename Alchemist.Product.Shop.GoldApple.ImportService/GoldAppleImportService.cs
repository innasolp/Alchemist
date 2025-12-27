using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Alchemist.Product.Shop.GoldApple.Model;
using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Products.Service;
using Import.Interfaces;

namespace Alchemist.Product.Shop.GoldApple.ImportService;

public class GoldAppleImportService(ILogger<GoldAppleImportService> logger,
    [FromKeyedServices(GoldAppleConstants.GolAppleKey)] string name,    
    [FromKeyedServices(GoldAppleConstants.GolAppleKey)] ILoaderService loader,
    string url,
    IEnumerable<IProductShopCategory> shopCategories,
    [FromKeyedServices(GoldAppleConstants.GolAppleKey)] IProductItemHandler productDataHandler,
    [FromKeyedServices(GoldAppleConstants.GolAppleKey)] string productUrlFormat,
    string categoryUrlFormat,
    string sourceName,
    ImportProductServiceOptions importProductServiceOptions
    )
    : ShopImportCategoryProductsService<CategoryProducts, ProductData>(logger,
        loader,
        url, 
        shopCategories, 
        productDataHandler,
        productUrlFormat,
        categoryUrlFormat, 
        sourceName,
        importProductServiceOptions
        )
{
    public override string Name { get; } = name;

    protected override bool? IsEndOfCategory(CategoryProducts category, int processProductCount)
    {
        return !(category.Data.Products?.Length > 0);
    }
}