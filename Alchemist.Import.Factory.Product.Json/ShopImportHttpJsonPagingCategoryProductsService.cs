using Alchemist.Import.Product.Json.Service;
using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Products.Json;
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
    : ShopImportJsonCategoryProductsService<PagingCategoryProducts, Product>(logger, loader, url, serviceName, shopCategories, itemHandler, 
        productUrlFormat, categoryUrlFormat, sourceName, 
        new CategoryItemUrlPaging<PagingCategoryProducts>(),
        importProductJsonServiceOptions)
{
    protected override string PreparePath(string path) => HttpUtility.UrlEncode(path);
}
