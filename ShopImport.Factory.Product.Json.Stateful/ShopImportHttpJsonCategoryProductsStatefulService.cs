using Alchemist.Import.Factory.Product.Json.Infrastructure;
using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Products.Json;
using Import.Interfaces;
using Microsoft.Extensions.Logging;
using ShopImport.KeyHash;
using ShopImport.Product.Service.Stateful;
using ShopImport.ServiceState;
using System.Web;

namespace ShopImport.Factory.Product.Json.Stateful;

internal class ShopImportHttpJsonCategoryProductsStatefulService(ILogger<ShopImportHttpJsonCategoryProductsStatefulService> logger,
    ILoaderService loader,
    string url,
    string serviceName,
    IEnumerable<IProductShopCategory> shopCategories,
    IProductItemHandler itemHandler,
    string productUrlFormat,
    string categoryUrlFormat,
    string sourceName,
    ImportProductJsonServiceOptions importProductJsonServiceOptions,
    IKeyHasher keyHasher,
    IServiceStateRepository serviceStateRepository)
    : ShopImportCategoryProductsStatefulService<CategoryProducts, Alchemist.Import.Factory.Product.Json.Infrastructure.Product>
    (logger, loader, url, shopCategories, itemHandler, productUrlFormat, categoryUrlFormat, sourceName, 
        new CategoryUrlPaging<CategoryProducts>(),
        new CustomCategoryJsonSerializer<CategoryProducts>(importProductJsonServiceOptions),
        importProductJsonServiceOptions,
        keyHasher,
        serviceStateRepository)
{
    public override string Name => serviceName;

    protected override string PreparePath(string path) => HttpUtility.UrlEncode(path);
}