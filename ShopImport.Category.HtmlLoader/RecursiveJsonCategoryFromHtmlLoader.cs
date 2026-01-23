using Alchemist.Import.Category.Interfaces;
using Import.Html;
using ShopImport.Category.Loader.Interfaces;
using ShopImport.Category.Recursive;
using System.Text.Json;
namespace ShopImport.Category.RecursiveHtmlLoader;

internal class RecursiveJsonCategoryFromHtmlLoader(IHtmlSearcher htmlSearcher, HtmlCategoryLoadOptions categoryLoadOptions)
    : ICategoryLoader
{
    public IHtmlSearcher HtmlSearcher { get; } = htmlSearcher;

    public bool IsRecursive { get; } = categoryLoadOptions.IsRecursive;

    public HtmlCategoryLoadOptions HtmlCategoryLoadOptions { get; } = categoryLoadOptions;

    ICategoryLoadOptions ICategoryLoader.CategoryLoadOptions => HtmlCategoryLoadOptions;

    async Task<IEnumerable<ICategory>> ICategoryLoader.LoadAsync(ICategory? parentCategory, Stream stream, CancellationToken cancellationToken = default)
    {
        RecursiveCategory? recursiveParentCategory = parentCategory is RecursiveCategory recursiveCategory
            ? recursiveCategory
            : parentCategory is null 
                ? null
                : throw new ArgumentException($"Expected {nameof(RecursiveCategory)} but got {parentCategory.GetType().Name}");         

        return await LoadRootElementAsync(recursiveParentCategory, stream, HtmlSearcher, HtmlCategoryLoadOptions, cancellationToken);
    }

    public static async Task<IEnumerable<ICategory>> LoadRootElementAsync(RecursiveCategory? parentCategory, 
        Stream stream, IHtmlSearcher htmlSearcher, HtmlCategoryLoadOptions categoryLoadStageOptions, CancellationToken cancellationToken = default)
    {
        var values = await htmlSearcher.GetValues(stream, categoryLoadStageOptions.HtmlSearchOptions, cancellationToken);
        if (values.Count == 0)
            return [];

        var rootElement = JsonDocument.Parse(values[0]).RootElement;      
        
        var elementHelper = new JsonElementHelper();    
        var categories = new List<ICategory>();
        await RecursiveCategory.LoadAllChildrenAsync(parentCategory, categories, rootElement,
                categoryLoadStageOptions.FirstNodePath,
                categoryLoadStageOptions.CategoryPropertyPaths,
                elementHelper,
                cancellationToken);

        return categories;
    }
}