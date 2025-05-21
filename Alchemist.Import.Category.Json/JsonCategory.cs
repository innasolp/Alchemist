using System.Text.Json;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text.Json.Serialization;
using Alchemist.Import.Common;
using Alchemist.Import.Category.Interfaces;

namespace Alchemist.Import.Category.Json;

public class JsonCategory : ICategory
{
    public int Id { get; private set; }

    public string Name { get; private set; }

    public string Description { get; private set; }

    public string Url { get; private set; }

    public int? ParentId { get; private set; }

    private readonly ObservableCollection<ICategory> _children = [];

    [JsonIgnore]
    public IEnumerable<ICategory> Children => _children;

    public bool? IsParented { get; private set; }

    public ICategory? ItemParent { get; private set; }

    private readonly List<int> _childrenIds = [];
    public IEnumerable<int> ChildrenIds => _childrenIds;

    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    private static readonly string _invalidDataMessageFormat = "Element '{0}' not contains {1} property '{2}'";

    private static readonly string _invalidUrlMessageFormat = "Url '{0}' not contains {1}";


    private JsonCategory()
    {
        _children.CollectionChanged += OnChildrenCollectionChanged;
    }

    private void OnChildrenCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        CollectionChanged?.Invoke(this, e);
    }

    private static bool TryGetProperty<T>(JsonElement categoryElement,
        PropertyPath propertyPath,
        Func<JsonElement, T> getValue,
        out T result)
    {
        result = default;
        if (categoryElement.TryGetProperty(propertyPath.Path, out JsonElement element))
        {
            result = getValue(element);
            return true;
        }
        else if (propertyPath.LoadStopIfNotExists == true)
            return false;
        else
            throw new InvalidDataException(string.Format(_invalidDataMessageFormat, categoryElement.GetRawText(), propertyPath.PropertyName, propertyPath.Path));
    }

    private delegate bool TryGetValueFromUrl<TResult>(string url, out TResult result);

    private static bool TryGetPropertyFromUrl<T>(JsonElement categoryElement,
        string propertyName,
        string? url,
        TryGetValueFromUrl<T> tryGetValueFromUrl,
        out T? result)
    {
        result = default;

        if (tryGetValueFromUrl(url, out result))
            return true;
        else
            throw new InvalidDataException(string.Format(_invalidUrlMessageFormat, url, propertyName));
    }

    public static bool TryLoad(JsonElement categoryElement, Dictionary<string, PropertyPath> propertyPathes, out JsonCategory category)
    {
        category = new JsonCategory();

        if (TryGetProperty(categoryElement, propertyPathes[nameof(Url)], (e) => e.GetString(), out string? url))
        {
            if (url == null) return false;
            category.Url = url;
        }
        else
            return false;

        if (propertyPathes.TryGetValue(nameof(Id), out PropertyPath idPath))
        {
            if (TryGetProperty(categoryElement, idPath, (e) => e.GetInt32(), out int id))
                category.Id = id;
            else
                return false;
        }
        else if (TryGetPropertyFromUrl(categoryElement, nameof(Id), category.Url, Utils.TryGetCategoryIdFromUrl, out int categoryId))
            category.Id = categoryId;

        if (propertyPathes.TryGetValue(nameof(Name), out PropertyPath namePath))
        {
            if (TryGetProperty(categoryElement, namePath, (e) => e.GetString(), out string? name))
                category.Name = name;
            else
                return false;
        }
        else
            category.Name = Utils.TryGetCategoryNameFromUrl(category.Url, out string categoryName) ? categoryName : "";


        if (propertyPathes.TryGetValue(nameof(Description), out PropertyPath descriptionPath))
        {
            if (TryGetProperty(categoryElement, descriptionPath, (e) => e.GetString(), out string? description))
                category.Description = description;
            else
                return false;
        }

        if (propertyPathes.TryGetValue(nameof(IsParented), out PropertyPath isParentedPath))
        {
            if (TryGetProperty(categoryElement, isParentedPath, (e) => e.GetBoolean(), out bool isParented))
                category.IsParented = isParented;
            else
                return false;
        }

        return true;
    }


    public static List<JsonCategory> LoadAllChildren(JsonCategory? parentCategory, JsonElement jsonElement, string[] nodePath, Dictionary<string, PropertyPath> propertyPathes)
    {
        try
        {
            var categoriesElements = GetAllElementsByNodePath(jsonElement, nodePath);

            var categoryElementArrays = categoriesElements.Where(e => e.ValueKind == JsonValueKind.Array);

            var endChildren = new List<JsonCategory>();

            if (!categoryElementArrays.Any())
            {
                foreach (var categoryElement in categoriesElements.Where(c => c.GetPropertyCount() > 0))
                {
                    if (!TryLoad(categoryElement, propertyPathes, out JsonCategory category))
                        continue;

                    endChildren.Add(category);

                    if (parentCategory?.Id > 0)
                        SetParent(category, parentCategory);

                    if (categoryElement.TryGetProperty(propertyPathes[nameof(Children)].Path, out JsonElement childrenElement)
                        && childrenElement.ValueKind == JsonValueKind.Array && childrenElement.GetArrayLength() > 0)
                        endChildren.AddRange(LoadChildrenTree(category, childrenElement, propertyPathes));
                }
            }
            else
                foreach (var categoryElement in categoryElementArrays)
                {
                    var children = LoadChildrenTree(parentCategory, categoryElement, propertyPathes);
                    endChildren.AddRange(children);
                }

            return endChildren;
        }
        catch (Exception e)
        {
            throw;
        }
    }
    public static void LoadAllChildren(JsonCategory? parentCategory, ICollection<JsonCategory> jsonCategories, JsonElement jsonElement, string[] nodePath, Dictionary<string, PropertyPath> propertyPathes)
    {
        try
        {
            var categoriesElements = GetAllElementsByNodePath(jsonElement, nodePath);

            var categoryElementArrays = categoriesElements.Where(e => e.ValueKind == JsonValueKind.Array);
            if (!categoryElementArrays.Any())
            {
                foreach (var categoryElement in categoriesElements.Where(c => c.GetPropertyCount() > 0))
                {
                    if (!TryLoad(categoryElement, propertyPathes, out JsonCategory category))
                        continue;

                    if (parentCategory?.Id > 0)
                        SetParent(category, parentCategory);

                    jsonCategories.Add(category);

                    if (categoryElement.TryGetProperty(propertyPathes[nameof(Children)].Path, out JsonElement childrenElement)
                        && childrenElement.ValueKind == JsonValueKind.Array && childrenElement.GetArrayLength() > 0)
                        LoadChildrenTree(category, jsonCategories, childrenElement, propertyPathes);
                }
            }
            else
                foreach (var categoryElement in categoryElementArrays)
                {
                    LoadChildrenTree(parentCategory, jsonCategories, categoryElement, propertyPathes);
                }
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    private static List<JsonElement> GetAllElementsByNodePath(JsonElement jsonElement, string[] nodePath)
    {
        var currentElement = jsonElement;
        var lastNodePath = new List<string>(nodePath);
        while (lastNodePath.Count > 0)
        {
            var path = lastNodePath.First();

            lastNodePath.Remove(path);

            if (!currentElement.TryGetProperty(path, out JsonElement nextElement))
                continue;

            currentElement = nextElement;

            if (currentElement.ValueKind == JsonValueKind.Array)
            {
                var arrayElements = currentElement.EnumerateArray();

                if (!arrayElements.Any()) continue;

                var elements = new List<JsonElement>();

                foreach (var element in arrayElements)
                {
                    elements.AddRange(GetAllElementsByNodePath(element, [.. lastNodePath]));
                }
                return elements;
            }
        }
        return [currentElement];
    }

    private static List<JsonCategory> LoadChildrenTree(JsonCategory? parentCategory,
        JsonElement categoriesElement,
        Dictionary<string, PropertyPath> propertyPathes)
    {
        if (categoriesElement.ValueKind != JsonValueKind.Array)
            throw new InvalidCastException($"element {categoriesElement.GetRawText()} is not json array.");

        var categoriesArray = categoriesElement.EnumerateArray();

        var endChildren = new List<JsonCategory>();

        foreach (var categoryElement in categoriesArray)
        {
            var isLoaded = TryLoad(categoryElement, propertyPathes, out JsonCategory category);

            if (category.Id == parentCategory?.Id)
                continue;

            if (isLoaded && parentCategory?.Id > 0)
                SetParent(category, parentCategory);

            endChildren.Add(category);

            if (categoryElement.TryGetProperty(propertyPathes[nameof(Children)].Path, out JsonElement childrenElement)
                && childrenElement.ValueKind == JsonValueKind.Array && childrenElement.GetArrayLength() > 0)            
                endChildren.AddRange(LoadChildrenTree(category, childrenElement, propertyPathes));               
        }

        return endChildren;
    }

    private static void LoadChildrenTree(JsonCategory? parentCategory, ICollection<JsonCategory> categories,
        JsonElement categoriesElement,
        Dictionary<string, PropertyPath> propertyPathes)
    {
        if (categoriesElement.ValueKind != JsonValueKind.Array)
            throw new InvalidCastException($"element {categoriesElement.GetRawText()} is not json array.");

        var categoriesArray = categoriesElement.EnumerateArray();
        
        foreach (var categoryElement in categoriesArray)
        {
            var isLoaded = TryLoad(categoryElement, propertyPathes, out JsonCategory category);

            if (category.Id == parentCategory?.Id)
                continue;

            if (isLoaded && parentCategory?.Id > 0)
                SetParent(category, parentCategory);

            categories.Add(category);

            if (categoryElement.TryGetProperty(propertyPathes[nameof(Children)].Path, out JsonElement childrenElement) &&
                childrenElement.ValueKind == JsonValueKind.Array && childrenElement.GetArrayLength() > 0)                      
               LoadChildrenTree(category, categories, childrenElement, propertyPathes);              
        }
    }

    private static void SetParent(JsonCategory category, JsonCategory parentCategory)
    {
        category.ParentId = parentCategory.Id;
        category.ItemParent = parentCategory;
        parentCategory._children.Add(category);
        parentCategory._childrenIds.Add(category.Id);
    }
}
