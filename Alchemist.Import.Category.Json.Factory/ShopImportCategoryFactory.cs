using Alchemist.Import.Category.Interfaces;
using Import.Html.Factory;
using Import.Interfaces;
using Import.Settings.Interfaces;
using Alchemist.Import.Settings.Extensions;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Import.Factory.Interfaces;
using Import.Factory.Service;
using Alchemist.Import.Settings.Category;
using Alchemist.Import.Category.Service;
using Import.Html;

namespace Alchemist.Import.Factory.Category;

public abstract class ShopImportCategoryFactory<TElement>(ILogger logger,
    ILoaderServiceFactory browserServiceFactory,
    ICategoryItemHandler itemHandler, 
    IImportServiceLogFactory? logFactory = null) 
    : ImportServiceFactory(logger, browserServiceFactory, logFactory)
{
    private readonly ICategoryItemHandler _itemHandler = itemHandler;

    protected override IImportService Create(ILogger logger, 
        string name,
        IImportSource shopModel,
        IImportSettings shopImportSettings,
        ILoaderService browserService)
    {
        if (shopImportSettings is not ICategoryShopImportSettings categoryShopImportSettings)
            throw new InvalidDataException($"Invalid settings type {shopImportSettings.GetType().Name}");

        if (shopModel is not ICategoryShopModel categoryShopModel)
            throw new InvalidOperationException($"Invalid type {shopModel.GetType()}. Must be implementation of {typeof(ICategoryShopModel)}");

        var loadOptionsService = (shopImportSettings.GetService(nameof(CategoryLoadOptions))?.Value) 
            ?? throw new InvalidDataException($"CategoryLoadOptions not exists for {shopModel.Name}");
        var categoryLoadOptions = JsonSerializer.Deserialize<CategoryLoadOptions>(loadOptionsService);
        
        var htmlSearchOptionsService = shopImportSettings.GetService(nameof(HtmlSearchFactoryOptions))?.Value;
        var htmlSearchFactoryOptions = htmlSearchOptionsService != null ? JsonSerializer.Deserialize<HtmlSearchFactoryOptions>(htmlSearchOptionsService) : null;

        var htmlSearcher = htmlSearchFactoryOptions != null 
            ? HtmlSearchFactory.CreateSearcher(htmlSearchFactoryOptions.SearchMatchType, htmlSearchFactoryOptions.SearchElementType)
            : null;

        return CreateShopImportCategoriesTimerService(logger, 
            name,
            htmlSearcher,
            browserService,
            categoryShopModel,
            categoryShopImportSettings,
            categoryLoadOptions, _itemHandler);
    }

    protected abstract ShopImportCategoriesTimerService<TElement> CreateShopImportCategoriesTimerService(ILogger logger, 
        string name, 
        IHtmlSearcher htmlSearcher, 
        ILoaderService loaderService,
        ICategoryShopModel categoryShopModel,
        ICategoryShopImportSettings categoryShopImportSettings,
        CategoryLoadOptions categoryLoadOptions,
        ICategoryItemHandler categoryItemHandler);
}