using Alchemist.Import.Product.JsonPathHandlers;
using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Products.Service;
using Import.Interfaces;
using Json.CustomSerialization;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Alchemist.Import.Product.Json.Service;

public class ShopImportJsonCategoryProductsService<TCategory, TProduct>
    : ShopImportCategoryProductsService<TCategory,TProduct>
    where TCategory : class, IJsonItem, ICategoryProducts, new()
    where TProduct : class, IJsonItem, IProductItem, new()
{   

    private readonly JsonLoader _productJsonLoader;
    private readonly JsonLoader _categoryJsonLoader;

    public ShopImportJsonCategoryProductsService(ILogger logger,
        ILoaderService loader,
        string url,
        string serviceName,
        IEnumerable<IProductShopCategory> shopCategories,
        IProductItemHandler itemHandler,
        string productUrlFormat,
        string categoryUrlFormat,
        string sourceName,
        ImportProductJsonServiceOptions importProductJsonServiceOptions)
        : base(logger, loader, url, shopCategories, itemHandler, productUrlFormat, categoryUrlFormat, sourceName, importProductJsonServiceOptions)
    {
        Name = serviceName;
        
        _productJsonLoader = new JsonLoader(importProductJsonServiceOptions.ProductJsonSettings);
        _categoryJsonLoader = new JsonLoader(importProductJsonServiceOptions.CategoryJsonSettings);
        _categoryJsonLoader.AddPathHandler(new PriceJsonPathHandler());
        _categoryJsonLoader.AddPathHandler(new CurrencyJsonPathHandler());
    }

    public override string Name { get; }

    protected override async Task<T?> DeserializeItemFromStream<T>(Stream stream, CancellationToken cancellationToken = default)
        where T : class
    {
        if ( typeof(T) == typeof(TCategory))
            return (await LoadFromJson<TCategory>(stream, _categoryJsonLoader, cancellationToken)) as T;

        if (typeof(T) == typeof(TProduct))
            return (await LoadFromJson<TProduct>(stream, _productJsonLoader, cancellationToken)) as T;

        return await base.DeserializeItemFromStream<T>(stream, cancellationToken);
    }

    private static async Task<T> LoadFromJson<T>(Stream stream, IJsonLoader jsonLoader, CancellationToken cancellationToken=default)
        where T:IJsonItem, new()
    {
        var json = await JsonSerializer.DeserializeAsync<JsonObject>(stream, cancellationToken: cancellationToken);
        var item = new T();
        jsonLoader.LoadFromJson(item, json);
        return item;
    }
}