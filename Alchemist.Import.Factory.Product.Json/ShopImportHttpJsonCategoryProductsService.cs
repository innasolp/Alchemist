using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Products.Json;
using Alchemist.Import.Products.Service;
using Import.Interfaces;
using Microsoft.Extensions.Logging;
using System.Web;

namespace Alchemist.Import.Factory.Product.Json;

internal class ShopImportHttpJsonCategoryProductsService(ILogger<ShopImportHttpJsonCategoryProductsService> logger,
    ILoaderService loader,
    string url,
    string serviceName,
    IEnumerable<IProductShopCategory> shopCategories,
    IProductItemHandler itemHandler,
    string productUrlFormat,
    string categoryUrlFormat,
    string sourceName,
    ImportProductJsonServiceOptions importProductJsonServiceOptions)
    : ShopImportCategoryProductsService<CategoryProducts, Product>(logger, loader, url, shopCategories, itemHandler, productUrlFormat, categoryUrlFormat, sourceName, 
        new CategoryUrlPaging<CategoryProducts>(),
        new CustomCategoryJsonSerializer<CategoryProducts>(importProductJsonServiceOptions),
        importProductJsonServiceOptions)
{
    public override string Name => serviceName;

    protected override string PreparePath(string path) => HttpUtility.UrlEncode(path);
}