using Alchemist.Import.Product.Json.Service;
using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Products.Json;
using Import.Interfaces;
using Json.CustomSerialization;
using Microsoft.Extensions.Logging;

namespace Alchemist.Import.Factory.Product.Json;

public class ShopImportJsonPaginatorCategoryProductsService(ILogger<ShopImportJsonPaginatorCategoryProductsService> logger, ILoaderService loader, string url, string serviceName, IEnumerable<IProductShopCategory> shopCategories, IProductItemHandler itemHandler, string productUrlFormat, string categoryUrlFormat, string sourceName, IJsonSettings productJsonSettings, IJsonSettings categoryJsonSettings, int? pageProductCount = null, object? productLoadData = null, object? categoryLoadData = null, UrlFormatType productUrlFormatType = UrlFormatType.Url, UrlFormatType categoryUrlFormatType = UrlFormatType.Url) 
    : ShopImportJsonPaginatorCategoryProductsService<PagingCategoryProducts, Import.Products.Json.Product>(logger, loader, url, serviceName, shopCategories, itemHandler, productUrlFormat, categoryUrlFormat, sourceName, productJsonSettings, categoryJsonSettings, pageProductCount, productLoadData, categoryLoadData, productUrlFormatType, categoryUrlFormatType)
{
}
