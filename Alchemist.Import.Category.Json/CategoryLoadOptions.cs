using Import.Html;

namespace Alchemist.Import.Category.Service;

public class CategoryLoadOptions
{
    public string Name { get; set; }

    public HtmlSearchOptions HtmlSearchOptions { get; set; }

    public string[] FirstNodePath { get; set; }

    public string? CategoriesApiUrlFormat { get; set; }

    public Dictionary<string, PropertyPath> CategoryPropertyPaths { get; set; }

    public int? SecondsInterval { get; set; }
}
