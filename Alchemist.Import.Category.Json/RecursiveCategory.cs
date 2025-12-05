using Alchemist.Import.Category.Interfaces;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;
using System.Text.Json.Serialization;

namespace Alchemist.Import.Category.Service;

public class RecursiveCategory : ICategory
{
    private static readonly string _invalidDataMessageFormat = "Element '{0}' not contains {1} property '{2}'";

    private static readonly string _invalidUrlMessageFormat = "Url '{0}' not contains {1}";
    public string Name { get; private set; }

    public string Url { get; private set; }

    public int Id { get; private set; }

    public string Description { get; private set; }

    public int? ParentId { get; private set; }

    private readonly ObservableCollection<ICategory> _children = [];

    [JsonIgnore]
    public IEnumerable<ICategory> Children => _children;

    private readonly List<int> _childrenIds = [];
    public IEnumerable<int> ChildrenIds => _childrenIds;

    private readonly SemaphoreSlim _categoriesSemaphoreSlim = new(1, 1);

    public bool? IsParented { get; private set; }

    public ICategory? ItemParent { get; private set; }

    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    protected RecursiveCategory()
    {
        _children.CollectionChanged += OnChildrenCollectionChanged;
    }

    private void OnChildrenCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        CollectionChanged?.Invoke(this, e);
    }

    protected async Task AddCategoryToCollectionAsync(ICollection<ICategory> jsonCategories, CancellationToken cancellationToken = default)
    {
        await _categoriesSemaphoreSlim.WaitAsync(cancellationToken);

        try
        {
            jsonCategories.Add(this);
        }
#if DEBUG
        catch (Exception e)
        {
            throw;
        }
#endif
        finally
        {
            _categoriesSemaphoreSlim.Release();
        }
    }

    private static bool TryLoadValuesFrom<TElement>(RecursiveCategory recursiveCategory, TElement categoryElement, Dictionary<string, PropertyPath> propertyPathes, IElementHelper<TElement> elementHelper)
    {
        if (TryGetProperty(categoryElement, propertyPathes[nameof(Url)], elementHelper.GetString, elementHelper, out string? url))
        {
            if (url == null) return false;
            recursiveCategory.Url = url;
        }
        else
            return false;

        if (propertyPathes.TryGetValue(nameof(Id), out var idPath))
        {
            if (TryGetProperty(categoryElement, idPath, elementHelper.GetInt32, elementHelper, out int id))
                recursiveCategory.Id = id;
            else
                return false;
        }
        else if (TryGetPropertyFromUrl(nameof(Id), recursiveCategory.Url, Utils.TryGetCategoryIdFromUrl, out int categoryId))
            recursiveCategory.Id = categoryId;

        if (propertyPathes.TryGetValue(nameof(Name), out var namePath))
        {
            if (TryGetProperty(categoryElement, namePath, elementHelper.GetString, elementHelper, out string? name))
                recursiveCategory.Name = name;
            else
                return false;
        }
        else
            recursiveCategory.Name = Utils.TryGetCategoryNameFromUrl(recursiveCategory.Url, out string categoryName) ? categoryName : "";


        if (propertyPathes.TryGetValue(nameof(Description), out var descriptionPath))
        {
            if (TryGetProperty(categoryElement, descriptionPath, elementHelper.GetString, elementHelper, out string? description))
                recursiveCategory.Description = description;
            else
                return false;
        }

        if (propertyPathes.TryGetValue(nameof(IsParented), out var isParentedPath))
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
        else if (propertyPath.LoadStopIfNotExists == true)
            return false;
        else
            throw new InvalidDataException(string.Format(_invalidDataMessageFormat,
                elementHelper.GetRawText(categoryElement), 
                propertyPath.PropertyName,
                propertyPath.Path));
    }

    private delegate bool TryGetValueFromUrl<TResult>(string url, out TResult result);

    private static bool TryGetPropertyFromUrl<T>(string propertyName,
        string? url,
        TryGetValueFromUrl<T> tryGetValueFromUrl,
        out T? result)
    {
        if (tryGetValueFromUrl(url, out result))
            return true;
        else
            throw new InvalidDataException(string.Format(_invalidUrlMessageFormat, url, propertyName));
    }

    private static async Task LoadChildrenTreeAsync<TElement>(RecursiveCategory? parentCategory, 
        ICollection<ICategory> recursiveCategories,
        IEnumerable<TElement> childrenCategoryElements,
        Dictionary<string, PropertyPath> propertyPathes,
        IElementHelper<TElement> elementHelper,
        CancellationToken cancellationToken)
    {
        var categoryElementQueue = new ConcurrentQueue<TElement>(childrenCategoryElements);

        while (!categoryElementQueue.IsEmpty)
        {
            if (!categoryElementQueue.TryDequeue(out var categoryElement))
                return;

            var category = new RecursiveCategory();
            var isLoaded = TryLoadValuesFrom(category, categoryElement, propertyPathes, elementHelper);

            if (category.Id == parentCategory?.Id)
                continue;

            if (isLoaded && parentCategory?.Id > 0)
                SetParent(category, parentCategory);

            await category.AddCategoryToCollectionAsync(recursiveCategories, cancellationToken);

            if (elementHelper.TryGetElementEnumerable(categoryElement, propertyPathes[nameof(Children)].Path, out var childrenElements))
                await LoadChildrenTreeAsync(category, recursiveCategories, childrenElements, propertyPathes, elementHelper, cancellationToken);
        }
    }

    protected static void SetParent(RecursiveCategory category, RecursiveCategory parentCategory)
    {
        category.ParentId = parentCategory.Id;
        category.ItemParent = parentCategory;
        parentCategory._children.Add(category);
        parentCategory._childrenIds.Add(category.Id);
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
        Dictionary<string, PropertyPath> propertyPathes,
        IElementHelper<TElement> elementHelper,
        CancellationToken cancellationToken)
    {
        try
        {
            var categoriesElements = nodePath?.Length > 0 ? GetAllElementsByNodePath(element, nodePath, elementHelper) : [element];

            await Task.WhenAll(categoriesElements.Select(async categoryElement =>
            {
                if (elementHelper.TryGetElementEnumerable(categoryElement, null, out var childElements))
                {
                    await LoadChildrenTreeAsync(parentCategory, categories, childElements, propertyPathes, elementHelper, cancellationToken);
                }
                else 
                {
                    var recursiveCategory = new RecursiveCategory();
                    TryLoadValuesFrom(recursiveCategory, categoryElement, propertyPathes, elementHelper);

                    if (parentCategory?.Id > 0)
                        SetParent(recursiveCategory, parentCategory);

                    await recursiveCategory.AddCategoryToCollectionAsync(categories, cancellationToken);

                    if (elementHelper.TryGetElementEnumerable(categoryElement, propertyPathes[nameof(Children)].Path, out childElements))
                        await LoadChildrenTreeAsync(recursiveCategory, categories, childElements, propertyPathes, elementHelper, cancellationToken);
                }
            }));
        }
        catch
        {
            throw;
        }
    }
}