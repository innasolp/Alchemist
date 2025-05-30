using System.Text.Json;

namespace Alchemist.Import.Category.Json;

public class JsonCategoryAsync : JsonCategory
{ 
    private readonly SemaphoreSlim _categoriesSemaphoreSlim = new(1, 1);
    protected async Task AddCategoryToCollectionAsync(ICollection<JsonCategory> jsonCategories, CancellationToken? cancellationToken = null)
    {
        if(cancellationToken != null)        
            await _categoriesSemaphoreSlim.WaitAsync(cancellationToken.Value);
        else 
            await _categoriesSemaphoreSlim.WaitAsync();

        try
        {
            jsonCategories.Add(this);
        }
        catch { throw; }
        finally
        {
            _categoriesSemaphoreSlim.Release();
        }
    }

    private static async Task LoadChildrenTreeAsync(JsonCategory? parentCategory, ICollection<JsonCategory> jsonCategories,
        JsonElement categoriesElement,
        Dictionary<string, PropertyPath> propertyPathes,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (categoriesElement.ValueKind != JsonValueKind.Array)
            throw new InvalidCastException($"element {categoriesElement.GetRawText()} is not json array.");

        var categoryElementQueue = new Queue<JsonElement>(categoriesElement.EnumerateArray());

        while (categoryElementQueue.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var categoryElement = categoryElementQueue.Dequeue();
            var category = new JsonCategoryAsync();
            var isLoaded = category.TryLoad(categoryElement, propertyPathes);

            if (category.Id == parentCategory?.Id)
                continue;

            if (isLoaded && parentCategory?.Id > 0)
                category.SetParent(parentCategory);

            await category.AddCategoryToCollectionAsync(jsonCategories, cancellationToken);

            if (categoryElement.TryGetProperty(propertyPathes[nameof(Children)].Path, out JsonElement childrenElement) &&
                childrenElement.ValueKind == JsonValueKind.Array && childrenElement.GetArrayLength() > 0)
                await LoadChildrenTreeAsync(category, jsonCategories, childrenElement, propertyPathes, cancellationToken);
        }
    }

    private static async Task LoadChildrenTreeFromElementAsync(JsonCategory? parentCategory,
        JsonElement categoryElement,
        ICollection<JsonCategory> jsonCategories,
        Dictionary<string, PropertyPath> propertyPathes,
        CancellationToken cancellationToken)
    {
        var jsonCategory = new JsonCategoryAsync();
        if (!jsonCategory.TryLoad(categoryElement, propertyPathes))
            return;

        if (parentCategory?.Id > 0)
            jsonCategory.SetParent(parentCategory);

        await jsonCategory.AddCategoryToCollectionAsync(jsonCategories, cancellationToken);

        if (categoryElement.TryGetProperty(propertyPathes[nameof(Children)].Path, out var childrenElement)
            && childrenElement.ValueKind == JsonValueKind.Array && childrenElement.GetArrayLength() > 0)
            await LoadChildrenTreeAsync(jsonCategory, jsonCategories, childrenElement, propertyPathes, cancellationToken);
    }

    public static async Task LoadAllChildrenAsync(JsonCategory? parentCategory,
        ICollection<JsonCategory> jsonCategories,
        JsonElement jsonElement,
        string[] nodePath,
        Dictionary<string, PropertyPath> propertyPathes,
        CancellationToken cancellationToken)
    {
        try
        {
            var categoriesElements = nodePath?.Length > 0 ? GetAllElementsByNodePath(jsonElement, nodePath) : [jsonElement];

            var categoryElementArrays = categoriesElements.Where(e => e.ValueKind == JsonValueKind.Array);
            if (!categoryElementArrays.Any())
            {
                await Task.WhenAll(categoriesElements.Where(c => c.GetPropertyCount() > 0).
                    Select(c => LoadChildrenTreeFromElementAsync(parentCategory, c, jsonCategories, propertyPathes, cancellationToken)));
            }
            else
                await Task.WhenAll(categoryElementArrays.
                    Select(c => LoadChildrenTreeAsync(parentCategory, jsonCategories, c, propertyPathes, cancellationToken)));
           
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
}
