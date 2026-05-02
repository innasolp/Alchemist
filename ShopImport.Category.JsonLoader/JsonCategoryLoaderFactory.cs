using ShopImport.Category.Loader.Interfaces;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ShopImport.Category.RecursiveJsonLoader;

internal class JsonCategoryLoaderFactory : ICategoryLoaderFactory
{
    ICategoryLoader ICategoryLoaderFactory.CreateCategoryLoader(ICategoryLoadOptions categoryLoadStageOptions)
    {
        if(categoryLoadStageOptions is not JsonCategoryLoadOptions jsonCategoryLoadStageOptions)        
            throw new ArgumentException($"Expected {nameof(JsonCategoryLoadOptions)} but received {categoryLoadStageOptions.GetType().Name}");
        
        return CreateJsonCategoryLoader(jsonCategoryLoadStageOptions);
    }

    public static JsonCategoryLoader CreateJsonCategoryLoader(JsonCategoryLoadOptions categoryLoadStageOptions)
    {
        return new JsonCategoryLoader(categoryLoadStageOptions);
    }

    ICategoryLoader ICategoryLoaderFactory.CreateCategoryLoader(JsonObject categoryLoadStageOptions)
    {
        return CreateCategoryLoader(categoryLoadStageOptions);
    }

    public static JsonCategoryLoader CreateCategoryLoader(JsonObject categoryLoadStageOptionsJson)
    {
        var categoryLoadStageOptions = JsonSerializer.Deserialize<JsonCategoryLoadOptions>(categoryLoadStageOptionsJson.ToJsonString());
        return categoryLoadStageOptions == null
            ? throw new ArgumentException("Failed to deserialize HtmlCategoryLoadStageOptions from JsonObject")
            : CreateJsonCategoryLoader(categoryLoadStageOptions);
    }
}