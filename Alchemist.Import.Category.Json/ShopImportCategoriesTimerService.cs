using Microsoft.Extensions.Logging;
using System.Text.Json;
using WebLoader.Interfaces;
using Microsoft.VisualStudio.Threading;
using Alchemist.Import.Service;
using System.Collections.ObjectModel;
using Alchemist.Import.Html;
using WebLoader.Common;
using Alchemist.Import.Interfaces;
using Alchemist.Import.Category.Interfaces;

namespace Alchemist.Import.Category.Json;

public class ShopImportCategoriesTimerService(ILogger<ShopImportCategoriesTimerService> logger,
    IHtmlSearcher? htmlSearcher,
    IWebLoader webLoader,
    IShopModel shop,
    RequestHeaders? requestHeaders,
    CategoryLoadOptions categoryLoadOptions,
   ICategoryItemHandler itemHandler)
    : ShopImportService(logger, webLoader, requestHeaders)
{
    protected IHtmlSearcher? HtmlSearcher { get; } = htmlSearcher;

    protected CategoryLoadOptions CategoryLoadOptions { get; } = categoryLoadOptions;

    public override string Name { get; } = categoryLoadOptions.Name;

    protected IShopModel ShopModel { get; } = shop;

    private readonly int _defaultInterval = 3600;

    private readonly IItemHandler _itemHandler = itemHandler;

    public ShopImportCategoriesTimerService(ILogger<ShopImportCategoriesTimerService> logger,
   IWebLoader webLoader,
   IShopModel shopUrlModel,
   RequestHeaders? requestHeaders,
   CategoryLoadOptions categoryLoadOptions,
   ICategoryItemHandler itemHandler)
        :this(logger, null, webLoader, shopUrlModel, requestHeaders, categoryLoadOptions, itemHandler)
    {        
    }

    protected virtual async Task LoadCategoriesAsync(CancellationToken stoppingToken)
    {
        if (!WebLoader.IsStarted)
            await StartWebLoaderIfNeedAsync(stoppingToken);

        if (!WebLoader.IsStarted) return;

        JsonDocument? document;

        if (HtmlSearcher != null)
        {
            var values = await ProcessUrlTaskAsync(LoadHtmlFromUrlAsync, ShopModel.Url);
            if (values == null) return;

            document = JsonDocument.Parse(values[0]);
        }
        else
            document = await ProcessUrlTaskAsync((url) => LoadFromUrlAsync(ShopModel.Url, stoppingToken), ShopModel.Url);

        var categories = new ObservableCollection<JsonCategory>();
        categories.CollectionChanged += CategoryCollectionChanged;

        JsonCategory.LoadAllChildren(null, categories, document.RootElement,
               CategoryLoadOptions.FirstNodePath,
               CategoryLoadOptions.CategoryPropertyPaths);

        if (CategoryLoadOptions.CategoriesApiUrlFormat == null)
            return;

        var parentCategories = new List<JsonCategory>(categories);
        foreach (var parentCategory in parentCategories)
        {
            var url = string.Format(CategoryLoadOptions.CategoriesApiUrlFormat, parentCategory.Id);

            var categoriesJson = await ProcessUrlTaskAsync((url) => LoadFromUrlAsync(url, stoppingToken), url);
            if (categoriesJson == null) return;

            JsonCategory.LoadAllChildren(parentCategory, categories,
                categoriesJson.RootElement,
                CategoryLoadOptions.FirstNodePath,
                 CategoryLoadOptions.CategoryPropertyPaths);
        }
    }

    private async Task<List<string>?> LoadHtmlFromUrlAsync(string url)
    {
        using var stream = await WebLoader.LoadFromUrl(ShopModel.Url);
        var values = await HtmlSearcher.GetValues(stream, CategoryLoadOptions.HtmlSearchOptions);
        stream.Close();
        return await Task.FromResult(values);
    }

    private async Task<JsonDocument?> LoadFromUrlAsync(string url, CancellationToken stoppingToken)
    {
        using var stream = await WebLoader.LoadFromUrl(url);

        var categoriesJson = await JsonDocument.ParseAsync(stream, cancellationToken: stoppingToken);

        stream.Close();

        return await Task.FromResult(categoriesJson);
    }

    private void CategoryCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
        {
            var joinableTaskFactory = new JoinableTaskFactory(new JoinableTaskContext());

            var newItems = e.NewItems?.OfType<JsonCategory>();
            if (newItems == null) return;

            foreach (var category in newItems)
            {
                joinableTaskFactory.Run(async () =>
                {
                    await _itemHandler.HandleItem(category, ShopModel);
                });

                Logger.LogInformation($"Category {category.Name}-{category.Id} for shop {ShopModel.Name} loaded");
            }
        }
    }

    public override async Task Start(CancellationToken stoppingToken)
    {
        try
        {
            await LoadCategoriesAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, ex.Message);
        }

        using PeriodicTimer timer = new(TimeSpan.FromSeconds(CategoryLoadOptions.SecondsInterval ?? _defaultInterval));

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await LoadCategoriesAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            Logger.LogInformation($"Timed Service {Name} is stopping.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, ex.Message);
        }
    }
}
