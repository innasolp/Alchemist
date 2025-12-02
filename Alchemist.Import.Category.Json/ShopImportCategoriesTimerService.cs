using Alchemist.Import.Category.Interfaces;
using Import.Html;
using Import.Interfaces;
using Import.Service;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Threading;
using System.Collections.ObjectModel;
using System.Text.Json;
namespace Alchemist.Import.Category.Json;

public class ShopImportCategoriesTimerService : ImportService
{
    protected sealed record ImportCategory(ICategory Category, string CategorySourceUrl, string SourceName, string SourceUrl) : IImportCategory
    {
    }

    protected IHtmlSearcher? HtmlSearcher { get; }

    protected CategoryLoadOptions CategoryLoadOptions { get; }

    public override string Name { get; }

    private readonly string _categorySourceUrl;

    private readonly string _sourceName;

    private readonly int _defaultInterval = 3600;

    private readonly ICategoryItemHandler _itemHandler;

    private readonly PeriodicTimer _timer;

    private bool? _isStarted;

    private readonly SemaphoreSlim _htmlLoaderSemaphoreSlim = new(1, 1);

    private readonly SemaphoreSlim _jsonLoaderSemaphoreSlim = new(1, 1);

    public ShopImportCategoriesTimerService(ILogger<ShopImportCategoriesTimerService> logger,
        string name,
        IHtmlSearcher? htmlSearcher,
        ILoaderService loader,
        string categorySourceUrl,
        string sourceName,
        string url,
        CategoryLoadOptions categoryLoadOptions,
       ICategoryItemHandler itemHandler) : base(logger, loader,  url)
    {
        HtmlSearcher = htmlSearcher;
        CategoryLoadOptions = categoryLoadOptions;
        Name = name;
        _categorySourceUrl = categorySourceUrl;
        _sourceName = sourceName;
        _itemHandler = itemHandler;

        _timer = new(TimeSpan.FromSeconds(CategoryLoadOptions.SecondsInterval ?? _defaultInterval));
    }

    public ShopImportCategoriesTimerService(ILogger<ShopImportCategoriesTimerService> logger,
        string name,
   ILoaderService loader,
   string categorySourceUrl,
   string sourceName,
   string url,
   CategoryLoadOptions categoryLoadOptions,
   ICategoryItemHandler itemHandler)
        : this(logger,name, null, loader, categorySourceUrl, sourceName, url, categoryLoadOptions, itemHandler)
    {
    }

    protected async Task<JsonDocument?> LoadJsonDocumentAsync(string url, CancellationToken cancellationToken)
    {
        if (HtmlSearcher != null)
        {
            var values = await LoadHtmlFromUrlAsync(url, cancellationToken);
            return values != null ? JsonDocument.Parse(values[0]) : null;
        }
        else
            return await LoadJsonFromUrlAsync(url, cancellationToken);
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
            var nextTickResult = await ProcessTaskAsync(() => _timer.WaitForNextTickAsync(stoppingToken).AsTask());  

            if(nextTickResult.Status == ResultStatus.Success && nextTickResult.Value)
             await LoadCategoriesAsync(stoppingToken);
        }
    }

    private async Task LoadCategoriesAsync(CancellationToken stoppingToken)
    {
        var documentResult = await ProcessUrlTaskAsync(LoadJsonDocumentAsync, _categorySourceUrl, stoppingToken);
        
        if (documentResult.Status != ResultStatus.Success || documentResult.Value == null)
        {            
            if (documentResult.Status == ResultStatus.Cancelled)
                Logger.LogInformation(ImportCategoryLogMessages.ServiceNotLoadedJsonDocFromUrlOperationWasCancelled,
                    [Name, _categorySourceUrl]);
            
            else if (documentResult.Status == ResultStatus.Error)
                Logger.LogError(ImportCategoryLogMessages.JsonLoadFromUrlFailed, [_categorySourceUrl, documentResult.Exception.Message]);
            
            else if (documentResult.Status == ResultStatus.Warning)
                Logger.LogInformation(ImportCategoryLogMessages.JsonDocumentNotLoadedFromUrlWithWarning,
                    [_categorySourceUrl, documentResult.Exception.Message]);

            return;
        }

        var categories = new ObservableCollection<JsonCategory>();
        categories.CollectionChanged += CategoryCollectionChanged;


        var loadParentCategoriesResult = await ProcessTaskAsync(() => JsonCategoryAsync.LoadAllChildrenAsync(null, categories, documentResult.Value.RootElement,
                CategoryLoadOptions.FirstNodePath,
                CategoryLoadOptions.CategoryPropertyPaths,
                stoppingToken));

        if (loadParentCategoriesResult.Status != ResultStatus.Success)
        {
            if (loadParentCategoriesResult.Status == ResultStatus.Cancelled)           
                Logger.LogInformation(ImportCategoryLogMessages.ServiceCancelledOnLoadingStartCategories, Name); 
            else if (loadParentCategoriesResult.Status == ResultStatus.Warning)
                Logger.LogWarning(ImportCategoryLogMessages.ServiceNotLoadedCategories, Name, loadParentCategoriesResult.Exception.Message);
            else if (loadParentCategoriesResult.Status == ResultStatus.Error)
                Logger.LogError(ImportCategoryLogMessages.ServiceNotLoadedCategories, Name, loadParentCategoriesResult.Exception.Message);
           
            return;
        }

        if (string.IsNullOrEmpty(CategoryLoadOptions.CategoriesApiUrlFormat))
        {
            categories.CollectionChanged -= CategoryCollectionChanged;
            return;
        }

        var parentCategories = new List<JsonCategory>(categories);
        var categoriesResult = await ProcessTaskAsync(() => Task.WhenAll(parentCategories.Select(c => LoadCategoryChildrentTreeAsync(c, CategoryLoadOptions.CategoriesApiUrlFormat, categories, stoppingToken))));
        if (categoriesResult.Status != ResultStatus.Success)
        {
            if (categoriesResult.Status == ResultStatus.Cancelled)
                Logger.LogInformation(ImportCategoryLogMessages.ServiceCancelledOnLoadingChildCategories, Name);
            else if (loadParentCategoriesResult.Status == ResultStatus.Warning)
                Logger.LogWarning(ImportCategoryLogMessages.ServiceNotLoadedChildCategories, Name, loadParentCategoriesResult.Exception.Message);
            else if (loadParentCategoriesResult.Status == ResultStatus.Error)
                Logger.LogError(ImportCategoryLogMessages.ServiceNotLoadedChildCategories, Name, loadParentCategoriesResult.Exception.Message);
        }

        categories.CollectionChanged -= CategoryCollectionChanged;
    }

    private async Task LoadCategoryChildrentTreeAsync(JsonCategory parentCategory, string urlFormat, ICollection<JsonCategory> categories, CancellationToken token)
    {
        var url = string.Format(urlFormat, parentCategory.Id);

        //todo if html?
        var categoriesJsonResult = await ProcessUrlTaskAsync(LoadJsonFromUrlAsync, url, token);
        if (categoriesJsonResult.Status == ResultStatus.Cancelled)
            token.ThrowIfCancellationRequested();

        if (categoriesJsonResult.Value == null || categoriesJsonResult.Status != ResultStatus.Success)
        {
            if (categoriesJsonResult.Status == ResultStatus.Warning)
                Logger.LogWarning(ImportCategoryLogMessages.ServiceCategoryFailedOnLoadingFromUrl, Name, parentCategory.Name, categoriesJsonResult.Exception.Message);
            else if (categoriesJsonResult.Status == ResultStatus.Error)
                Logger.LogError(ImportCategoryLogMessages.ServiceCategoryFailedOnLoadingFromUrl, Name, parentCategory.Name, categoriesJsonResult.Exception.Message);

            if (categoriesJsonResult.Exception != null)
                throw categoriesJsonResult.Exception;
        }

        await JsonCategoryAsync.LoadAllChildrenAsync(parentCategory, categories,
            categoriesJsonResult.Value.RootElement,
            CategoryLoadOptions.FirstNodePath,
             CategoryLoadOptions.CategoryPropertyPaths,
             token);        
    }

    private async Task<List<string>?> LoadHtmlFromUrlAsync(string url, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        await _htmlLoaderSemaphoreSlim.WaitAsync(token);
        try
        {
            using var stream = await LoadFromUrlAsync(url, token);
            var values = await HtmlSearcher.GetValues(stream, CategoryLoadOptions.HtmlSearchOptions, token);
            stream.Close();
            return await Task.FromResult(values);
        }
#if DEBUG
        catch
        {
            throw;
        }
#endif  
        finally
        {
            _htmlLoaderSemaphoreSlim.Release();
        }
    }

    private async Task<JsonDocument?> LoadJsonFromUrlAsync(string url, CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        await _jsonLoaderSemaphoreSlim.WaitAsync(stoppingToken);

        try
        { 
            using var stream = await LoadFromUrlAsync(url, stoppingToken);
            var categoriesJson = await JsonDocument.ParseAsync(stream, cancellationToken: stoppingToken);
            return await Task.FromResult(categoriesJson);
        }
#if DEBUG
        catch
        {
            throw;
        }
#endif  
        finally
        {
            _jsonLoaderSemaphoreSlim.Release();
        }       
    }

    private readonly CancellationToken _currentCancellationToken = default;
    
    private readonly SemaphoreSlim _categoryCollectionChangedSemaphore = new (1,1);

    private async void CategoryCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.Action != System.Collections.Specialized.NotifyCollectionChangedAction.Add)return;

        await _categoryCollectionChangedSemaphore.WaitAsync(_currentCancellationToken);

        try
        {
            var newItems = e.NewItems?.OfType<JsonCategory>();
            if (newItems == null) return;

            foreach (var category in newItems)
            {
                Logger.LogInformation(ImportCategoryLogMessages.CategoryNameIdForShopWasLoaded,
                    category.Name, category.Id, _sourceName);

                await _itemHandler.HandleItem(new ImportCategory(category, _categorySourceUrl, _sourceName, Host ), _currentCancellationToken);

                Logger.LogInformation(ImportCategoryLogMessages.CategoryNameIdForShopWasHandled,
                    category.Name, category.Id, _sourceName);
            }
        }
        catch(OperationCanceledException operationCanceled) 
        {
            Logger.LogInformation(operationCanceled, "Item handling canceled.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Item handling error.");
        }
        finally
        {
            _categoryCollectionChangedSemaphore.Release();
        }
    }
}