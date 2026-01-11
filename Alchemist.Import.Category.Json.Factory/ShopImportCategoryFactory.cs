using Alchemist.Import.Category.Interfaces;
using Alchemist.Import.Category.Service;
using Alchemist.Import.Settings;
using Alchemist.Import.Settings.Category;
using Alchemist.Import.Settings.Extensions;
using Import.Factory.Interfaces;
using Import.Factory.Service;
using Import.Html;
using Import.Html.Factory;
using Import.Interfaces;
using Import.Settings.Interfaces;
using Microsoft.Extensions.Logging;

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

        if (shopModel is not ICategoryShopSource categoryShopModel)
            throw new InvalidOperationException($"Invalid type {shopModel.GetType()}. Must be implementation of {typeof(ICategoryShopSource)}");

        var loadOptionsService = shopImportSettings.GetService(nameof(CategoryLoadOptions)) 
            ?? throw new InvalidDataException($"CategoryLoadOptions not exists for {shopModel.Name}");
        var categoryLoadOptions = loadOptionsService.GetServiceValue<CategoryLoadOptions>();
        
        var htmlSearchOptionsService = shopImportSettings.GetService(nameof(HtmlSearchFactoryOptions));
        var htmlSearchFactoryOptions = htmlSearchOptionsService?.GetServiceValue<HtmlSearchFactoryOptions>();

        var htmlSearcher = htmlSearchFactoryOptions != null 
            ? HtmlSearchFactory.CreateSearcher(htmlSearchFactoryOptions.SearchMatchType, htmlSearchFactoryOptions.SearchElementType)
            : null;

        var categoryDataService = shopImportSettings.GetService("RequestOptions");
        var categoryLoadData = categoryDataService?.GetServiceValue<RequestOptions>();

        return CreateShopImportCategoriesTimerService(logger, 
            name,
            htmlSearcher,
            browserService,
            categoryShopModel,
            categoryShopImportSettings,
            categoryLoadOptions, _itemHandler, categoryLoadData);
    }

    protected abstract ShopImportCategoriesTimerService<TElement> CreateShopImportCategoriesTimerService(ILogger logger, 
        string name, 
        IHtmlSearcher htmlSearcher, 
        ILoaderService loaderService,
        ICategoryShopSource categoryShopModel,
        ICategoryShopImportSettings categoryShopImportSettings,
        CategoryLoadOptions categoryLoadOptions,
        ICategoryItemHandler categoryItemHandler,
        object? categoryLoadData = null);
}