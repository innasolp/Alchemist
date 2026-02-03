using Alchemist.Import.Category.Interfaces;
using System.Collections.Concurrent;
using System.Text.Json.Serialization;

namespace ShopImport.Category.Recursive;

public class RecursiveCategory : ICategory, IDisposable
{
    public string Name { get;  }

    public string Url { get; }

    public int Id { get; }

    public string? Description { get; private set; }

    public int? ParentId { get; private set; }

    private readonly List<ICategory> _children = [];

    [JsonIgnore]
    public IEnumerable<ICategory> Children => _children;

    private readonly SemaphoreSlim _categoriesSemaphoreSlim = new(1, 1);   

    public bool? IsParented { get; private set; }

    public ICategory? ItemParent { get; private set; }

    protected RecursiveCategory(int id, string name, string url)
    {       
        Id = id;
        Name = name;
        Url = url;
    }

    protected async Task AddCategoryToCollectionAsync(ICollection<ICategory> jsonCategories, CancellationToken cancellationToken = default)
    {
        await _categoriesSemaphoreSlim.WaitAsync(cancellationToken);

        try
        {
            jsonCategories.Add(this);
        }
        finally
        {
            _categoriesSemaphoreSlim.Release();
        }
    }

    private static bool TryGetRecursiveCategoryFrom<TElement>(TElement categoryElement,
        Dictionary<string, PropertyPath> propertyPaths,
        IElementHelper<TElement> elementHelper,
        out RecursiveCategory? recursiveCategory)
    {
        recursiveCategory = default;

        if (!propertyPaths.TryGetValue(nameof(Url), out var urlPath))
            return false;

        if(!TryGetProperty(categoryElement, urlPath, elementHelper.GetString, elementHelper, out var url)
            || string.IsNullOrEmpty(url))
            url = "";

        if ((!propertyPaths.TryGetValue(nameof(Id), out var idPath) 
            || !TryGetProperty(categoryElement, idPath, elementHelper.GetInt32, elementHelper, out int id))
            && !Utils.TryGetCategoryIdFromUrl(url, out id))
            return false;


        if (!propertyPaths.TryGetValue(nameof(Name), out var namePath)
            || !TryGetProperty(categoryElement, namePath, elementHelper.GetString, elementHelper, out var name)
            || string.IsNullOrEmpty(name))
            return false;

        recursiveCategory = new RecursiveCategory(id, name, url);

        if (propertyPaths.TryGetValue(nameof(Description), out var descriptionPath)
            && TryGetProperty(categoryElement, descriptionPath, elementHelper.GetString, elementHelper, out string? description))
                recursiveCategory.Description = description;

        if (propertyPaths.TryGetValue(nameof(IsParented), out var isParentedPath))
        {
            if (TryGetProperty(categoryElement, isParentedPath, elementHelper.GetBoolean, elementHelper, out bool isParented))
                recursiveCategory.IsParented = isParented;
            else
                return false;
        }

        return true;
    }

    private static bool TryGetProperty<TElement, T>(TElement categoryElement,
        PropertyPath propertyPath,
        Func<TElement, T> getValue,
        IElementHelper<TElement> elementHelper,
        out T? result)
    {
        result = default;
        if (elementHelper.TryGetByProperty(categoryElement, propertyPath.Path, out TElement element))
        {
            result = getValue(element);
            return true;
        }
        else
            return false;
    }

    private static async Task LoadChildrenTreeAsync<TElement>(RecursiveCategory? parentCategory, 
        ICollection<ICategory> recursiveCategories,
        IEnumerable<TElement> childrenCategoryElements,
        Dictionary<string, PropertyPath> propertyPaths,
        IElementHelper<TElement> elementHelper,
        CancellationToken cancellationToken)
    {
        var categoryElementQueue = new ConcurrentQueue<TElement>(childrenCategoryElements);

        while (!categoryElementQueue.IsEmpty)
        {
            if (!categoryElementQueue.TryDequeue(out var categoryElement))
                return;

            if(!TryGetRecursiveCategoryFrom(categoryElement, propertyPaths, elementHelper, out var category) 
                || category is null)
                continue;

            if (category.Id == parentCategory?.Id)
                continue;

            if (parentCategory?.Id > 0)
                await SetParentAsync(category, parentCategory, cancellationToken);

            await category.AddCategoryToCollectionAsync(recursiveCategories, cancellationToken);

            if (elementHelper.TryGetElementEnumerable(categoryElement, propertyPaths[nameof(Children)].Path, out var childrenElements))
                await LoadChildrenTreeAsync(category, recursiveCategories, childrenElements, propertyPaths, elementHelper, cancellationToken);
        }
    }

    protected static async Task SetParentAsync(RecursiveCategory category, RecursiveCategory parentCategory, CancellationToken cancellationToken)
    {
        category.ParentId = parentCategory.Id;
        category.ItemParent = parentCategory;

        await category.AddCategoryToCollectionAsync(parentCategory._children, cancellationToken);
    }

    protected static IEnumerable<TElement> GetAllElementsByNodePath<TElement>(TElement element, string[] nodePath, IElementHelper<TElement> elementHelper)
    {
        var currentElement = element;
        var lastNodePath = new List<string>(nodePath);
        while (lastNodePath.Count > 0)
        {
            var path = lastNodePath.First();

            lastNodePath.Remove(path);

            if (elementHelper.TryGetElementEnumerable(currentElement, path, out var innerElements))
            {
                var elements = new List<TElement>();
                foreach(var innerElement in innerElements.Where(e=>!elementHelper.IsEmpty(e)))
                {
                    elements.AddRange(GetAllElementsByNodePath(innerElement, [..lastNodePath], elementHelper));
                }
                return elements;
            }
            else if(elementHelper.TryGetByProperty(currentElement, path, out var nextElement))
            {
                currentElement = nextElement;
                continue;
            }
        }
        return [currentElement];
    }

    public static async Task LoadAllChildrenAsync<TElement>(RecursiveCategory? parentCategory,
        ICollection<ICategory> categories,
        TElement element,
        string[] nodePath,
        Dictionary<string, PropertyPath> propertyPaths,
        IElementHelper<TElement> elementHelper,
        CancellationToken cancellationToken)
    {
        var categoriesElements = nodePath?.Length > 0 ? GetAllElementsByNodePath(element, nodePath, elementHelper) : [element];

        await Task.WhenAll(categoriesElements.Select(async categoryElement =>
        {
            if (elementHelper.TryGetElementEnumerable(categoryElement, null, out var childElements))
            {
                await LoadChildrenTreeAsync(parentCategory, categories, childElements, propertyPaths, elementHelper, cancellationToken);
            }
            else
            {
                if (!TryGetRecursiveCategoryFrom(categoryElement, propertyPaths, elementHelper, out var recursiveCategory)
                    || recursiveCategory is null)
                    return;

                if (parentCategory?.Id > 0)
                    await SetParentAsync(recursiveCategory, parentCategory, cancellationToken);

                await recursiveCategory.AddCategoryToCollectionAsync(categories, cancellationToken);

                if (elementHelper.TryGetElementEnumerable(categoryElement, propertyPaths[nameof(Children)].Path, out childElements))
                    await LoadChildrenTreeAsync(recursiveCategory, categories, childElements, propertyPaths, elementHelper, cancellationToken);
            }
        }));
    }

    void IDisposable.Dispose()
    {
        _categoriesSemaphoreSlim.Dispose();
        foreach(var category in _children.OfType<IDisposable>())
        {
            category.Dispose();
        }
    }
}