using Alchemist.Import.Category.Interfaces;
using Import.Html;
using Import.Interfaces;
using Import.Service;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Threading;
using System.Collections.ObjectModel;

namespace Alchemist.Import.Category.Service;

public abstract class ShopImportCategoriesTimerService<TElement> : ImportService
{
    protected sealed record ImportCategory(ICategory Category, string CategorySourceUrl, string SourceName, string SourceUrl) : IImportCategory
    {
    }

    protected abstract IElementHelper<TElement> GetElementHelper();

    protected IHtmlSearcher? HtmlSearcher { get; }

    protected CategoryLoadOptions CategoryLoadOptions { get; }

    public override string Name { get; }

    private readonly string _categorySourceUrl;

    private readonly string _sourceName;

    private readonly int _defaultInterval = 3600;

    private readonly ICategoryItemHandler _itemHandler;

    private readonly PeriodicTimer _timer;

    private bool? _isStarted;

    private readonly SemaphoreSlim _htmlSearcherSemaphoreSlim = new(1, 1);

    private readonly object? _categoryLoadData;

    public ShopImportCategoriesTimerService(ILogger logger,
        string name,
        IHtmlSearcher? htmlSearcher,
        ILoaderService loader,
        string categorySourceUrl,
        string sourceName,
        string url,
        CategoryLoadOptions categoryLoadOptions,
       ICategoryItemHandler itemHandler,
       object? categoryLoadData = null) : base(logger, loader, url)
    {
        HtmlSearcher = htmlSearcher;
        CategoryLoadOptions = categoryLoadOptions;
        Name = name;
        _categorySourceUrl = categorySourceUrl;
        _sourceName = sourceName;
        _itemHandler = itemHandler;

        _timer = new(TimeSpan.FromSeconds(CategoryLoadOptions.SecondsInterval ?? _defaultInterval));
        _categoryLoadData = categoryLoadData;
    }

    public ShopImportCategoriesTimerService(ILogger logger,
        string name,
   ILoaderService loader,
   string categorySourceUrl,
   string sourceName,
   string url,
   CategoryLoadOptions categoryLoadOptions,
   ICategoryItemHandler itemHandler,
        object? categoryLoadData = null)
        : this(logger, name, null, loader, categorySourceUrl, sourceName, url, categoryLoadOptions, itemHandler, categoryLoadData)
    {
    }

    protected async Task<(bool success, TElement? result)> TryLoadElementAsync(string url, CancellationToken cancellationToken = default)
    {
        var categoryLoadData = LoadData != null
                ? new object?[] { LoadData, _categoryLoadData }
                : LoadData;

        if (HtmlSearcher != null)
        {
            var (success, values) = await TryLoadHtmlFromUrlAsync(url, categoryLoadData, cancellationToken);

            if (!success) return (false, default(TElement?));

            return (true, LoadElementFromString(values[0]));
        }
        else
            return await TryLoadElementFromUrlAsync(url, categoryLoadData, cancellationToken);
    }

    protected override async Task ProcessAsync(CancellationToken stoppingToken)
    {
        if (_isStarted == null)
        {
            _isStarted = true;
            await LoadCategoriesAsync(stoppingToken);
        }
        else if (!stoppingToken.IsCancellationRequested)
        {
            var nextTickResult = await _timer.WaitForNextTickAsync(stoppingToken).AsTask();

            await LoadCategoriesAsync(stoppingToken);
        }
    }

    private async Task LoadCategoriesAsync(CancellationToken stoppingToken)
    {
        var (success, elementResult) = await TryLoadElementAsync(_categorySourceUrl, stoppingToken);

        if (!success)
        {
            Logger.LogInformation(ImportCategoryLogMessages.CategoriesWereNotLoaded, _sourceName, Host);
            return;
        }

        var categories = new ObservableCollection<ICategory>();
        categories.CollectionChanged += CategoryCollectionChanged;

        var elementHelper = GetElementHelper();

        await RecursiveCategory.LoadAllChildrenAsync(null, categories, elementResult,
                CategoryLoadOptions.FirstNodePath,
                CategoryLoadOptions.CategoryPropertyPaths,
                elementHelper,
                stoppingToken);


        if (string.IsNullOrEmpty(CategoryLoadOptions.CategoriesApiUrlFormat))
        {
            categories.CollectionChanged -= CategoryCollectionChanged;
            return;
        }

        var parentCategories = new List<ICategory>(categories);
        var loadChildCategoriesTasks = parentCategories.Select(c => LoadCategoryChildrentTreeAsync(c as RecursiveCategory,
            CategoryLoadOptions.CategoriesApiUrlFormat, categories, elementHelper, stoppingToken));
        
        await Task.WhenAll(loadChildCategoriesTasks);

        categories.CollectionChanged -= CategoryCollectionChanged;
    }

    private async Task LoadCategoryChildrentTreeAsync(RecursiveCategory parentCategory, 
        string urlFormat, 
        ICollection<ICategory> categories, 
        IElementHelper<TElement> elementHelper,
        CancellationToken token)
    {
        var url = string.Format(urlFormat, parentCategory.Id);        

        var (success, categoriesResult) = await TryLoadElementFromUrlAsync(url, LoadData, token);

        if (!success)
        {
            Logger.LogInformation(ImportCategoryLogMessages.ChildrenForCategoryWereNotLoaded, url);
            return;
        }

        try
        {
            await RecursiveCategory.LoadAllChildrenAsync(parentCategory,
                categories,
                categoriesResult,
                CategoryLoadOptions.FirstNodePath,
                 CategoryLoadOptions.CategoryPropertyPaths,
                 elementHelper,
                 token);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, ex.Message);
            Logger.LogInformation(ImportCategoryLogMessages.ChildrenForCategoryWereNotLoaded, url);
        }
    }

    private async Task<(bool success, List<string>? result)> TryLoadHtmlFromUrlAsync(string url, object? requestData = null, CancellationToken token = default)
    {
        var (success, stream) = await TryLoadFromUrlAsync(url, requestData, token);

        using (stream)
        {
            if (!success)
                return await Task.FromResult((false, default(List<string>)));

            try
            {
                await _htmlSearcherSemaphoreSlim.WaitAsync(token);
                var values = await HtmlSearcher.GetValues(stream, CategoryLoadOptions.HtmlSearchOptions, token);
                return await Task.FromResult((true, values));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ImportCategoryLogMessages.ParseHtmlFromUrlFailed, url, ex.Message);
                return await Task.FromResult((false, default(List<string>)));
            }
            finally
            {
                stream?.Close();
                _htmlSearcherSemaphoreSlim.Release();
            }
        }
    }

    private async Task<(bool success, TElement? element)> TryLoadElementFromUrlAsync(string url, object? requestData = null, CancellationToken stoppingToken = default)
    {
        var (success, stream) = await TryLoadFromUrlAsync(url, requestData, stoppingToken);

        if (!success)        
            return (false, default(TElement));

        try
        {
            using (stream)
            {
                var categoriesElement = await LoadElementFromStreamAsync(stream, stoppingToken);
                stream.Close();
                return await Task.FromResult((true, categoriesElement));
            }
        }
        catch (SerializationException ex)
        {
            Logger.LogError(ex, ImportCategoryLogMessages.ParseFromUrlFailed, url, ex.Message);
            return await Task.FromResult((false, default(TElement)));
        }
    }

    protected abstract Task<TElement> LoadElementFromStreamAsync(Stream stream, CancellationToken cancellationToken);

    protected abstract TElement LoadElementFromString(string str);

    private readonly SemaphoreSlim _categoryCollectionChangedSemaphore = new(1, 1);

    private async void CategoryCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.Action != System.Collections.Specialized.NotifyCollectionChangedAction.Add) return;

        await _categoryCollectionChangedSemaphore.WaitAsync();

        try
        {
            var newItems = e.NewItems?.OfType<RecursiveCategory>();
            if (newItems == null) return;

            foreach (var category in newItems)
            {
                Logger.LogInformation(ImportCategoryLogMessages.CategoryNameIdForShopWasLoaded,
                    category.Name, category.Id, _sourceName);

                await _itemHandler.HandleItem(new ImportCategory(category, _categorySourceUrl, _sourceName, Host));

                Logger.LogInformation(ImportCategoryLogMessages.CategoryNameIdForShopWasHandled,
                    category.Name, category.Id, _sourceName);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, ImportCategoryLogMessages.ItemHandlingError);
        }
        finally
        {
            _categoryCollectionChangedSemaphore.Release();
        }
    }
}