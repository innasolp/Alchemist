using ShopImport.Category.Loader.Interfaces;
using ShopImport.Category.Recursive;

namespace ShopImport.Category.RecursiveJsonLoader;

public class JsonCategoryLoadOptions : ICategoryLoadOptions
{
    public Dictionary<string, PropertyPath> CategoryPropertyPaths { get; set; }

    public string? CategoriesApiUrlFormat { get; set; }

    public string[] FirstNodePath { get; set; }

    public bool IsRecursive {  get; set; } = false;
}