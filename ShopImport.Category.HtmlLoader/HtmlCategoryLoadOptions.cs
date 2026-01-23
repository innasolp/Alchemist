using Import.Html;
using Import.Html.Factory;
using ShopImport.Category.Loader.Interfaces;
using ShopImport.Category.Recursive;

namespace ShopImport.Category.RecursiveHtmlLoader;

public class HtmlCategoryLoadOptions : ICategoryLoadOptions
{
    public HtmlSearchOptions HtmlSearchOptions { get; set; }

    public HtmlSearchFactoryOptions HtmlSearchFactoryOptions { get; set; }

    public Dictionary<string, PropertyPath> CategoryPropertyPaths { get; set; }

    public string? CategoriesApiUrlFormat { get; set; }

    public string[] FirstNodePath { get; set; }

    public bool IsRecursive { get; set; } = false;
}
