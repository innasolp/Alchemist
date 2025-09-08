using Alchemist.Import.Category.Interfaces;
using Alchemist.Import.Html;
using Alchemist.Import.Interfaces;
using Alchemist.Import.Service;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Threading;
using System.Collections.ObjectModel;
using System.Text.Json;
namespace Alchemist.Import.Category.Json;

public class ShopImportCategoriesTimerService : ImportService
{
    protected sealed record ImportCategory(ICategory Category, ICategoryShopModel CategoryShopModel) : IImportCategory
    {
    }

    protected IHtmlSearcher? HtmlSearcher { get; }

    protected CategoryLoadOptions CategoryLoadOptions { get; }

    public override string Name { get; }

    protected ICategoryShopModel ShopModel { get; }

    private readonly int _defaultInterval = 3600;

    private readonly ICategoryItemHandler _itemHandler;

    private readonly PeriodicTimer _timer;

    private bool? _isStarted;

    public ShopImportCategoriesTimerService(ILogger<ShopImportCategoriesTimerService> logger,
        IHtmlSearcher? htmlSearcher,
        ILoaderService loader,
        ICategoryShopModel shop,
        CategoryLoadOptions categoryLoadOptions,
       ICategoryItemHandler itemHandler) : base(logger, loader,  shop.Host)
    {
        HtmlSearcher = htmlSearcher;
        CategoryLoadOptions = categoryLoadOptions;
        Name = categoryLoadOptions.Name;
        ShopModel = shop;
        _itemHandler = itemHandler;

        _timer = new(TimeSpan.FromSeconds(CategoryLoadOptions.SecondsInterval ?? _defaultInterval));
    }

    public ShopImportCategoriesTimerService(ILogger<ShopImportCategoriesTimerService> logger,
   ILoaderService loader,
   ICategoryShopModel shopUrlModel,
   CategoryLoadOptions categoryLoadOptions,
   ICategoryItemHandler itemHandler)
        : this(logger, null, loader, shopUrlModel, categoryLoadOptions, itemHandler)
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

    protected override async Task ProcessAsync(CancellationToken stoppingToken, CancellationTokenSource serviceStopiingToken)
    {
        if (_isStarted == null)
        {
            _isStarted = true;
            await LoadCategoriesAsync(stoppingToken);
        }
        else if (!stoppingToken.IsCancellationRequested)
        {            
            var nextTickResult = await ProcessTaskAsync(() => _timer.WaitForNextTickAsync(stoppingToken).AsTask());  

            if(nextTickResult.Status == Common.ResultStatus.Success && nextTickResult.Value)
             await LoadCategoriesAsync(stoppingToken);
        }
    }

    private async Task LoadCategoriesAsync(CancellationToken stoppingToken)
    {
        var documentResult = await ProcessUrlTaskAsync((url) => LoadJsonDocumentAsync(url, stoppingToken), ShopModel.CategorySourceUrl);
        
        if (documentResult.Status != Common.ResultStatus.Success || documentResult.Value == null)
        {            
            if (documentResult.Status == Common.ResultStatus.Cancelled)
                Logger.LogInformation(ImportCategoryLogMessages.ServiceNotLoadedJsonDocFromUrlOperationWasCancelled,
                    [Name, ShopModel.CategorySourceUrl]);
            
            else if (documentResult.Status == Common.ResultStatus.Error)
                Logger.LogError(ImportCategoryLogMessages.JsonLoadFromUrlFailed, [ShopModel.CategorySourceUrl, documentResult.Exception.Message]);
            
            else if (documentResult.Status == Common.ResultStatus.Warning)
                Logger.LogInformation(ImportCategoryLogMessages.JsonDocumentNotLoadedFromUrlWithWarning,
                    [ShopModel.CategorySourceUrl, documentResult.Exception.Message]);

            return;
        }

        var categories = new ObservableCollection<JsonCategory>();
        categories.CollectionChanged += CategoryCollectionChanged;


        var loadParentCategoriesResult = await ProcessTaskAsync(() => JsonCategoryAsync.LoadAllChildrenAsync(null, categories, documentResult.Value.RootElement,
                CategoryLoadOptions.FirstNodePath,
                CategoryLoadOptions.CategoryPropertyPaths,
                stoppingToken));

        if (loadParentCategoriesResult.Status != Common.ResultStatus.Success)
        {
            if (loadParentCategoriesResult.Status == Common.ResultStatus.Cancelled)           
                Logger.LogInformation(ImportCategoryLogMessages.ServiceCancelledOnLoadingStartCategories, Name); 
            else if (loadParentCategoriesResult.Status == Common.ResultStatus.Warning)
                Logger.LogWarning(ImportCategoryLogMessages.ServiceNotLoadedCategories, Name, loadParentCategoriesResult.Exception.Message);
            else if (loadParentCategoriesResult.Status == Common.ResultStatus.Error)
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
        if (categoriesResult.Status != Common.ResultStatus.Success)
        {
            if (categoriesResult.Status == Common.ResultStatus.Cancelled)
                Logger.LogInformation(ImportCategoryLogMessages.ServiceCancelledOnLoadingChildCategories, Name);
            else if (loadParentCategoriesResult.Status == Common.ResultStatus.Warning)
                Logger.LogWarning(ImportCategoryLogMessages.ServiceNotLoadedChildCategories, Name, loadParentCategoriesResult.Exception.Message);
            else if (loadParentCategoriesResult.Status == Common.ResultStatus.Error)
                Logger.LogError(ImportCategoryLogMessages.ServiceNotLoadedChildCategories, Name, loadParentCategoriesResult.Exception.Message);
        }

        categories.CollectionChanged -= CategoryCollectionChanged;
    }

    private async Task LoadCategoryChildrentTreeAsync(JsonCategory parentCategory, string urlFormat, ICollection<JsonCategory> categories, CancellationToken token)
    {
        var url = string.Format(urlFormat, parentCategory.Id);

        //todo if html?
        var categoriesJsonResult = await ProcessUrlTaskAsync((url) => LoadJsonFromUrlAsync(url, token), url);
        if (categoriesJsonResult.Status == Common.ResultStatus.Cancelled)
            token.ThrowIfCancellationRequested();

        if (categoriesJsonResult.Value == null || categoriesJsonResult.Status != Common.ResultStatus.Success)
        {
            if (categoriesJsonResult.Status == Common.ResultStatus.Warning)
                Logger.LogWarning(ImportCategoryLogMessages.ServiceCategoryFailedOnLoadingFromUrl, Name, parentCategory.Name, categoriesJsonResult.Exception.Message);
            else if (categoriesJsonResult.Status == Common.ResultStatus.Error)
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

        using var stream = await LoadFromUrlAsync(url); 
        var values = await HtmlSearcher.GetValues(stream, CategoryLoadOptions.HtmlSearchOptions, token);
        stream.Close();
        return await Task.FromResult(values);
    }

    private async Task<JsonDocument?> LoadJsonFromUrlAsync(string url, CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();       

        using var stream = await LoadFromUrlAsync(url);

        try
        {
            var categoriesJson = await JsonDocument.ParseAsync(stream, cancellationToken: stoppingToken);
            return await Task.FromResult(categoriesJson);
        }
        catch { throw; }
        finally
        {
            stream.Close();
        }       
    }

    private readonly object _categoryCollectionChangedLock = new();
    private void CategoryCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
        {
            lock(_categoryCollectionChangedLock)
            {
                var newItems = e.NewItems?.OfType<JsonCategory>();
                if (newItems == null) return;

                var joinableTaskFactory = new JoinableTaskFactory(new JoinableTaskContext());

                foreach (var category in newItems)
                {
                    Logger.LogInformation(ImportCategoryLogMessages.CategoryNameIdForShopWasLoaded,
                        category.Name, category.Id, ShopModel.ShopName);

                    joinableTaskFactory.Run(async () =>
                    {
                        await _itemHandler.HandleItem(new ImportCategory(category, ShopModel));
                    });

                    Logger.LogInformation(ImportCategoryLogMessages.CategoryNameIdForShopWasHandled,
                        category.Name, category.Id, ShopModel.ShopName);
                }
            }
        }
    }

    public override ValueTask DisposeAsync()
    {
        _timer.Dispose();

        return base.DisposeAsync();
    }
}
