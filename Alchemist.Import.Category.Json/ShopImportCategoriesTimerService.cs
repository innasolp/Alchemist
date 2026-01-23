using Alchemist.Import.Category.Interfaces;
using Import.Interfaces;
using Import.Service;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Threading;
using ShopImport.Category.Loader.Interfaces;
using System.Collections.ObjectModel;

namespace Alchemist.Import.Category.Service;

public class ShopImportCategoriesTimerService : ImportService
{
    protected sealed record ImportCategory(ICategory Category, string CategorySourceUrl, string SourceName, string SourceUrl) : IImportCategory;

    public override string Name { get; }

    private readonly string _categorySourceUrl;

    private readonly string _sourceName;

    private readonly int _defaultInterval = 3600;

    private readonly ICategoryItemHandler _itemHandler;

    private readonly PeriodicTimer _timer;

    private bool? _isStarted;

    private readonly object? _categoryLoadData;

    private readonly IEnumerable<ICategoryLoader> _categoryLoadStages;

    public ShopImportCategoriesTimerService(ILogger logger,
        string name,
        ILoaderService loader,
        string categorySourceUrl,
        string sourceName,
        string url,
       IEnumerable<ICategoryLoader> categoryLoadStages,
       ICategoryItemHandler itemHandler,
       CategoryImportOptions? categoryImportOptions = null,
       object? categoryLoadData = null) : base(logger, loader, url)
    {
        Name = name;
        _categorySourceUrl = categorySourceUrl;
        _sourceName = sourceName;
        _categoryLoadStages = categoryLoadStages;
        _itemHandler = itemHandler;

        _timer = new(TimeSpan.FromSeconds(categoryImportOptions?.SecondsInterval ?? _defaultInterval));
        _categoryLoadData = categoryLoadData;
    } 

    protected override async Task ProcessAsync(CancellationToken stoppingToken)
    {
        var categoryLoadData = LoadData != null && _categoryLoadData != null 
                ? new object?[] { LoadData, _categoryLoadData } 
                : LoadData;

        if (_isStarted == null)
        {
            _isStarted = true;
            await LoadCategoriesAsync(categoryLoadData, stoppingToken);
        }
        else if (!stoppingToken.IsCancellationRequested)
        {
            var nextTickResult = await _timer.WaitForNextTickAsync(stoppingToken).AsTask();

            await LoadCategoriesAsync(categoryLoadData, stoppingToken);
        }
    }

    private async Task LoadCategoriesAsync(object? categoryLoadData, CancellationToken stoppingToken)
    {
        var allCategories = new ObservableCollection<ICategory>();
        allCategories.CollectionChanged += CategoryCollectionChanged;

        var parentCategories = new List<ICategory>();

        foreach (var stage in _categoryLoadStages)
        {
            if (parentCategories.Count == 0)
            {
                var (success, loadedCategories) = await LoadCategoryChildrenAsync(_categorySourceUrl, categoryLoadData, stage, null, cancellationToken : stoppingToken);
                if (!success)
                {
                    Logger.LogInformation(ImportCategoryLogMessages.CategoriesWereNotLoaded, _sourceName, Host);
                    return;
                }
                
                parentCategories.AddRange(loadedCategories.Where(c=>!c.Children.Any()));
                loadedCategories.ToList().ForEach(allCategories.Add);                
            }
            else if(!string.IsNullOrEmpty(stage.CategoryLoadOptions.CategoriesApiUrlFormat))
            {
                var loadedCategories = await LoadCategoryChildrenAsync(stage.CategoryLoadOptions.CategoriesApiUrlFormat, 
                    stage, parentCategories, allCategories, categoryLoadData, stoppingToken);
                parentCategories = [.. loadedCategories.Where(c => !c.Children.Any())];
            }
        }

        allCategories.CollectionChanged -= CategoryCollectionChanged;
    }

    private async Task<IEnumerable<ICategory>> LoadCategoryChildrenAsync(string urlFormat, ICategoryLoader stage, IEnumerable<ICategory> parentCategories,
        IList<ICategory> allCategories, object? categoryLoadData, CancellationToken stoppingToken)        
    {
        var anyLoaded = false;

        List<ICategory> currentCategories = [];

        async Task loadParentCategoryStageAsync(ICategory parentCategory)
        {
            var url = string.Format(urlFormat, parentCategory.Id);
            var (success, loadedCategories) = await LoadCategoryChildrenAsync(url, categoryLoadData, stage, parentCategory, !stage.IsRecursive,
                cancellationToken: stoppingToken);

            anyLoaded = anyLoaded | success;
            if (!success) return;

            currentCategories.AddRange(loadedCategories);
            loadedCategories.ToList().ForEach(allCategories.Add);
        }

        var loadChildCategoriesTasks = parentCategories.Select(loadParentCategoryStageAsync);

        await Task.WhenAll(loadChildCategoriesTasks);        

        IEnumerable<ICategory> loadedParentCategories = [.. currentCategories.Where(c => !c.Children.Any())];

        if (stage.IsRecursive && anyLoaded)
        {
            var loadedCategories = await LoadCategoryChildrenAsync(urlFormat, stage, loadedParentCategories, allCategories, categoryLoadData, stoppingToken);
            currentCategories.AddRange(loadedCategories);
        }
       
        return currentCategories;
    }

    private async Task<(bool success, IEnumerable<ICategory> result)> LoadCategoryChildrenAsync(string url, 
        object? categoryLoadData,
        ICategoryLoader stage,
        ICategory? parentCategory, 
        bool required = true,
        CancellationToken cancellationToken = default)
    {
        var (success, stream) = await TryLoadFromUrlAsync(url, categoryLoadData, cancellationToken);

        if (!success)
        {
            if(required)
                Logger.LogInformation(ImportCategoryLogMessages.CategoryWasNotLoaded, url);

            return (false, []);
        }

        try
        {
            var loadedCategories = await stage.LoadAsync(parentCategory, stream, cancellationToken);          
            return (true, loadedCategories);    
        }
        catch (Exception e)
        {
            Logger.LogError(e, e.Message);
            Logger.LogInformation(ImportCategoryLogMessages.ChildrenForCategoryWereNotLoaded, url);
            return (false, []);
        }
    }    
    
    private readonly SemaphoreSlim _categoryCollectionChangedSemaphore = new(1, 1);

    private async void CategoryCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.Action != System.Collections.Specialized.NotifyCollectionChangedAction.Add) return;

        await _categoryCollectionChangedSemaphore.WaitAsync();

        try
        {
            var newItems = e.NewItems?.OfType<ICategory>();
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