using Microsoft.Extensions.Logging;
using System.Text.Json;
using WebLoader.Interfaces;
using Microsoft.VisualStudio.Threading;
using Alchemist.Import.Service;
using System.Collections.ObjectModel;
using Alchemist.Import.Html;
using WebLoader.Common;
using Alchemist.Import.Category.Interfaces;

namespace Alchemist.Import.Category.Json;

public class ShopImportCategoriesTimerService : ShopImportService
{
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
        IWebLoader webLoader,
        ICategoryShopModel shop,
        RequestHeaders? requestHeaders,
        CategoryLoadOptions categoryLoadOptions,
       ICategoryItemHandler itemHandler) : base(logger, webLoader, requestHeaders)
    {
        HtmlSearcher = htmlSearcher;
        CategoryLoadOptions = categoryLoadOptions;
        Name = categoryLoadOptions.Name;
        ShopModel = shop;
        _itemHandler = itemHandler;

        _timer = new(TimeSpan.FromSeconds(CategoryLoadOptions.SecondsInterval ?? _defaultInterval));
    }

    public ShopImportCategoriesTimerService(ILogger<ShopImportCategoriesTimerService> logger,
   IWebLoader webLoader,
   ICategoryShopModel shopUrlModel,
   RequestHeaders? requestHeaders,
   CategoryLoadOptions categoryLoadOptions,
   ICategoryItemHandler itemHandler)
        : this(logger, null, webLoader, shopUrlModel, requestHeaders, categoryLoadOptions, itemHandler)
    {
    }

    protected async Task<JsonDocument?> LoadJsonDocumentAsync(string url, CancellationToken cancellationToken)
    {
        if (HtmlSearcher != null)
        {
            var values = await LoadHtmlFromUrlAsync(url, cancellationToken);
            return JsonDocument.Parse(values[0]);
        }
        else
            return await LoadJsonDocumentFromUrlAsync(url, cancellationToken);
    }


    protected override async Task<bool> ExecutingCancellationNeeded(CancellationToken stoppingToken)
    {
        return await base.ExecutingCancellationNeeded(stoppingToken)
            || (_isStarted != null && !await _timer.WaitForNextTickAsync(stoppingToken));
    }    

    protected override async Task ProcessAsync(CancellationToken stoppingToken)
    {
        if(_isStarted == null)
            _isStarted = true;

        var documentResult = await ProcessUrlTaskAsync((url) => LoadJsonDocumentAsync(url, stoppingToken), ShopModel.CategorySourceUrl);
        if (documentResult.Status != Alchemist.Common.Status.Success || documentResult.Result == null)
        {
            //todo log error
            return;
        }

        var categories = new ObservableCollection<JsonCategory>();
        categories.CollectionChanged += CategoryCollectionChanged;

        await JsonCategoryAsync.LoadAllChildrenAsync(null, categories, documentResult.Result.RootElement,
               CategoryLoadOptions.FirstNodePath,
               CategoryLoadOptions.CategoryPropertyPaths,
               stoppingToken);

        if (string.IsNullOrEmpty(CategoryLoadOptions.CategoriesApiUrlFormat))
        {
            categories.CollectionChanged -= CategoryCollectionChanged;
            return;
        }

        var parentCategories = new List<JsonCategory>(categories);
        await Task.WhenAll(parentCategories.Select(c=>LoadCategoryChildrentTreeAsync(c, CategoryLoadOptions.CategoriesApiUrlFormat, categories, stoppingToken)));

        categories.CollectionChanged -= CategoryCollectionChanged;
    }

    private async Task LoadCategoryChildrentTreeAsync(JsonCategory parentCategory, string urlFormat, ICollection<JsonCategory> categories, CancellationToken token)
    {
        var url = string.Format(urlFormat, parentCategory.Id);

        var categoriesJsonResult = await ProcessUrlTaskAsync((url) => LoadJsonDocumentFromUrlAsync(url, token), url);
        if (categoriesJsonResult.Result == null || categoriesJsonResult.Status == Alchemist.Common.Status.Error)
        {
            //todo log error
            return;
        }

        await JsonCategoryAsync.LoadAllChildrenAsync(parentCategory, categories,
            categoriesJsonResult.Result.RootElement,
            CategoryLoadOptions.FirstNodePath,
             CategoryLoadOptions.CategoryPropertyPaths,
             token);
    }

    private async Task<List<string>?> LoadHtmlFromUrlAsync(string url, CancellationToken token)
    {
        using var stream = await LoadFromUrlAsync(url); 
        var values = await HtmlSearcher.GetValues(stream, CategoryLoadOptions.HtmlSearchOptions, token);
        stream.Close();
        return await Task.FromResult(values);
    }

    private async Task<JsonDocument?> LoadJsonDocumentFromUrlAsync(string url, CancellationToken stoppingToken)
    {
        using var stream = await LoadFromUrlAsync(url);

        var categoriesJson = await JsonDocument.ParseAsync(stream, cancellationToken: stoppingToken);

        stream.Close();

        return await Task.FromResult(categoriesJson);
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
                    joinableTaskFactory.Run(async () =>
                    {
                        await _itemHandler.HandleItem(category, ShopModel);
                    });

                    Logger.LogInformation($"Category {category.Name}-{category.Id} for shop {ShopModel.ShopName} handled");
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
