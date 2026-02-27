using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Products.Json;
using Alchemist.Import.Products.Service;
using Import.Interfaces;
using Microsoft.Extensions.Logging;
using System.Web;

namespace Alchemist.Import.Factory.Product.Json;

internal class ShopImportHttpJsonPagingCategoryProductsService(ILogger<ShopImportHttpJsonPagingCategoryProductsService> logger, 
    ILoaderService loader,
    string url,
    string serviceName,
    IEnumerable<IProductShopCategory> shopCategories, 
    IProductItemHandler itemHandler,
    string productUrlFormat,
    string categoryUrlFormat,
    string sourceName,
    ImportProductJsonServiceOptions importProductJsonServiceOptions) 
    : ShopImportCategoryProductsService<PagingCategoryProducts, Product>(logger, loader, url, shopCategories, itemHandler, 
        productUrlFormat, categoryUrlFormat, sourceName, 
        new CategoryItemUrlPaging<PagingCategoryProducts>(),
        new CustomCategoryJsonSerializer<PagingCategoryProducts>(importProductJsonServiceOptions),
        importProductJsonServiceOptions)
{
    public override string Name => serviceName;

    protected override string PreparePath(string path) => HttpUtility.UrlEncode(path);
}