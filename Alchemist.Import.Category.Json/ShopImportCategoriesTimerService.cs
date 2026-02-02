using Alchemist.Import.Category.Interfaces;
using Import.Interfaces;
using Import.Service;
using Microsoft.Extensions.Logging;
using ShopImport.Category.Loader.Interfaces;

namespace Alchemist.Import.Category.Service;

public class ShopImportCategoriesTimerService : ImportService, IDisposable, IAsyncDisposable
{
    protected sealed record ImportCategory(ICategory Category, string CategorySourceUrl, string SourceName, string SourceUrl) : IImportCategory;

    public override string Name { get; }

    private readonly string _categorySourceUrl;

    private readonly string _sourceName;

    private readonly int _defaultInterval = 3600;

    private readonly ICategoryItemHandler _itemHandler;

    private readonly PeriodicTimer _timer;

    private bool? _initialLoadDone;
    private bool disposedValue;
    private readonly object? _categoryLoadData;

    private readonly IEnumerable<ICategoryLoader> _categoryLoadStages;

    private readonly SemaphoreSlim _handleSemaphore = new(8);

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
        var loaderData = GetLoaderData();
        var categoryLoadData = loaderData != null && _categoryLoadData != null
                ? new object?[] { loaderData, _categoryLoadData }
                : loaderData;

        // renamed flag to avoid shadowing base class
        if (_initialLoadDone == null)
        {
            _initialLoadDone = true;
            await LoadCategoriesAsync(categoryLoadData, stoppingToken);            
        }

        while ((await _timer.WaitForNextTickAsync(stoppingToken).AsTask()) && !stoppingToken.IsCancellationRequested)
        {
            await LoadCategoriesAsync(categoryLoadData, stoppingToken);
        }
    }

    private async Task LoadCategoriesAsync(object? categoryLoadData, CancellationToken stoppingToken)
    {
        var parentCategories = new List<ICategory>();

        foreach (var stage in _categoryLoadStages)
        {
            if (parentCategories.Count == 0)
            {
                var (success, loadedCategories) = await LoadCategoryChildrenAsync(_categorySourceUrl, categoryLoadData, stage, null, cancellationToken: stoppingToken);
                if (!success)
                {
                    Logger.LogInformation(ImportCategoryLogMessages.CategoriesWereNotLoaded, _sourceName, Host);
                    return;
                }

                parentCategories.AddRange(loadedCategories.Where(c => !c.Children.Any()));

                foreach (var category in loadedCategories)
                {
                    Logger.LogInformation(ImportCategoryLogMessages.CategoryNameIdForShopWasLoaded,
                    category.Name, category.Id, _sourceName);
                }

                await Task.WhenAll(loadedCategories.Select(c => HandleCategoryAsync(c, stoppingToken)));
            }
            else if (!string.IsNullOrEmpty(stage.CategoryLoadOptions.CategoriesApiUrlFormat))
            {
                var loadedCategories = await LoadCategoryChildrenAsync(stage.CategoryLoadOptions.CategoriesApiUrlFormat,
                    stage, parentCategories, categoryLoadData, stoppingToken);

                await Task.WhenAll(loadedCategories.Select(c => HandleCategoryAsync(c, stoppingToken)));

                parentCategories = loadedCategories.Where(c => !c.Children.Any()).ToList();
            }
        }
    }

    private async Task<IEnumerable<ICategory>> LoadCategoryChildrenAsync(string urlFormat, ICategoryLoader stage, IEnumerable<ICategory> parentCategories,
        object? categoryLoadData, CancellationToken stoppingToken)
    {
        var anyLoaded = false;

        List<ICategory> currentCategories = [];

        var loaderDegree = 8;
        await Parallel.ForEachAsync(parentCategories, new ParallelOptions { CancellationToken = stoppingToken, MaxDegreeOfParallelism = loaderDegree },
            async (parentCategory, ct) =>
            {
                var url = string.Format(urlFormat, parentCategory.Id);
                var (success, loadedCategories) = await LoadCategoryChildrenAsync(url, categoryLoadData, stage, parentCategory, !stage.IsRecursive,
                    cancellationToken: ct);
            });

        IEnumerable<ICategory> loadedParentCategories = [.. currentCategories.Where(c => !c.Children.Any())];

        if (stage.IsRecursive && anyLoaded)
        {
            var loadedCategories = await LoadCategoryChildrenAsync(urlFormat, stage, loadedParentCategories, categoryLoadData, stoppingToken);
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
            if (required)
                Logger.LogInformation(ImportCategoryLogMessages.CategoryWasNotLoaded, url);

            return (false, Enumerable.Empty<ICategory>());
        }

        try
        {
            using (stream)
            {
                var loadedCategories = await stage.LoadAsync(parentCategory, stream, cancellationToken);
                stream.Close();
                return (true, loadedCategories);
            }
        }
        catch(OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            Logger.LogError(e, e.Message);
            Logger.LogInformation(ImportCategoryLogMessages.ChildrenForCategoryWereNotLoaded, url);
            return (false, Enumerable.Empty<ICategory>());
        }
    }

    private async Task HandleCategoryAsync(ICategory category, CancellationToken cancellationToken)
    {
        await _handleSemaphore.WaitAsync(cancellationToken);
        try
        {
            await _itemHandler.HandleItem(new ImportCategory(category, _categorySourceUrl, _sourceName, Host), cancellationToken);

            Logger.LogInformation(ImportCategoryLogMessages.CategoryNameIdForShopWasHandled,
                category.Name, category.Id, _sourceName);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, ImportCategoryLogMessages.ItemHandlingError);
        }
        finally
        {
            _handleSemaphore.Release();
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
                _timer.Dispose();
                _handleSemaphore.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    void IDisposable.Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    ValueTask IAsyncDisposable.DisposeAsync()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }
}