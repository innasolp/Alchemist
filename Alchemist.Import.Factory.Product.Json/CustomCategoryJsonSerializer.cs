using Alchemist.Import.Product.JsonPathHandlers;
using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Products.Service;
using Json.CustomSerialization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Alchemist.Import.Factory.Product.Json;

internal class CustomCategoryJsonSerializer<TCategory> : IItemSerializer<TCategory>
    where TCategory : class, ICategoryProducts, IJsonItem, new()
{
    private readonly JsonLoader _categoryJsonLoader;

    public CustomCategoryJsonSerializer(ImportProductJsonServiceOptions importProductJsonServiceOptions)
    {
        _categoryJsonLoader = new JsonLoader(importProductJsonServiceOptions.CategoryJsonSettings);
        _categoryJsonLoader.AddPathHandler(new PriceJsonPathHandler());
        _categoryJsonLoader.AddPathHandler(new CurrencyJsonPathHandler());
    }

    private static async Task<T?> LoadFromJson<T>(Stream stream, IJsonLoader jsonLoader, CancellationToken cancellationToken = default)
        where T : IJsonItem, new()
    {
        var json = await JsonSerializer.DeserializeAsync<JsonObject>(stream, cancellationToken: cancellationToken);
        var item = new T();
        jsonLoader.LoadFromJson(item, json);
        return item;
    }

    public Task<TCategory?> DeserializeFromStream(Stream stream, CancellationToken cancellationToken = default)
    {
        return LoadFromJson<TCategory>(stream, _categoryJsonLoader, cancellationToken);
    }
}