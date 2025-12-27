using Alchemist.Product.Shop.Ozon.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Products.Service;
using Import.Interfaces;
namespace Alchemist.Product.Shop.Ozon.ImportService;

public class OzonImportService(ILogger<OzonImportService> logger,
    [FromKeyedServices(OzonImportServiceConstants.OzonKey)] string name,
    [FromKeyedServices(OzonImportServiceConstants.OzonKey)] ILoaderService loaderService, 
    string url,  
    IEnumerable<IProductShopCategory> shopCategories,
    [FromKeyedServices(OzonImportServiceConstants.OzonKey)] IProductItemHandler itemHandler,
    string productUrlFormat,
        string categoryUrlFormat,
        string sourceName, 
        ImportProductServiceOptions importProductServiceOptions
   )
    : ShopImportPaginatorCategoryProductsService<Category, Model.Product>(logger,
        loaderService,
        url,
        shopCategories, 
        itemHandler,
        productUrlFormat, categoryUrlFormat,
        sourceName,
        importProductServiceOptions)
{
    public override string Name { get; } = name;

    protected override bool? IsEndOfCategory(Category category, int processProductCount)
    {
        return base.IsEndOfCategory(category, processProductCount) ?? category.CategoryContent == null; ;
    }
}