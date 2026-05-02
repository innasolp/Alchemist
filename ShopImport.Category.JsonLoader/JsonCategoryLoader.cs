using Alchemist.Import.Category.Interfaces;
using ShopImport.Category.Loader.Interfaces;
using ShopImport.Category.Recursive;
using System.Text.Json;

namespace ShopImport.Category.RecursiveJsonLoader;

internal class JsonCategoryLoader(JsonCategoryLoadOptions jsonCategoryLoadOptions) : ICategoryLoader
{
    public bool IsRecursive { get; } = jsonCategoryLoadOptions.IsRecursive;

    public JsonCategoryLoadOptions JsonCategoryLoadOptions { get; } = jsonCategoryLoadOptions;

    ICategoryLoadOptions ICategoryLoader.CategoryLoadOptions => JsonCategoryLoadOptions;

    public static async Task<IEnumerable<ICategory>> LoadRootElementAsync(Stream stream, JsonCategoryLoadOptions categoryLoadStageOptions, CancellationToken cancellationToken = default)
    {
        var jsonDocument = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        var elementHelper = new JsonElementHelper();
        var categories = new List<ICategory>();
        await RecursiveCategory.LoadAllChildrenAsync(null, categories, jsonDocument.RootElement,
                categoryLoadStageOptions.FirstNodePath,
                categoryLoadStageOptions.CategoryPropertyPaths,
                elementHelper,
                cancellationToken);

        return categories;
    }

    Task<IEnumerable<ICategory>> ICategoryLoader.LoadAsync(ICategory? parentCategory, Stream stream, CancellationToken cancellationToken)
    {
        return LoadRootElementAsync(stream, JsonCategoryLoadOptions, cancellationToken);
    }
}