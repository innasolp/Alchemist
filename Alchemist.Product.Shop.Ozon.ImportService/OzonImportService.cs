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
    object? productLoadData = null,
    object? categoryLoadData = null,
    UrlFormatType productUrlFormatType = default,
    UrlFormatType categoryUrlFormatType = default
   )
    : ShopImportPaginatorCategoryProductsService<Category, Model.Product>(logger,
        loaderService,
        url,
        shopCategories, 
        itemHandler,
        productUrlFormat, categoryUrlFormat, sourceName,
        productLoadData,
        categoryLoadData,
        productUrlFormatType,
        categoryUrlFormatType)
{
    public override string Name { get; } = name;

    protected override int PageProductCount => 12;

    protected override bool IsEndOfCategory(Category category)
    {
        return category.CategoryContent == null;
    }
}