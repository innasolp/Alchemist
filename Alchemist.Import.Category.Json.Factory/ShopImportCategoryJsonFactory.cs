using Alchemist.Import.Category.Interfaces;
using Alchemist.Import.Category.Json;
using Alchemist.Import.Factory.BrowserService;
using Alchemist.Import.Factory.Logging;
using Alchemist.Import.Html.Factory;
using Alchemist.Import.Interfaces;
using Alchemist.Import.Service.Factory.Abstractions;
using Alchemist.Import.Settings.Interfaces;
using Alchemist.Import.Settings.Extensions;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Alchemist.Import.Factory.Category.Json;

public class ShopImportCategoryJsonFactory(ILogger<ShopImportCategoriesTimerService> logger,
    ILoaderServiceFactory browserServiceFactory,
    ICategoryItemHandler itemHandler, 
    IImportServiceLogFactory? logFactory = null) 
    : ShopImportServiceFactory(logger, browserServiceFactory, logFactory)
{
    private readonly ICategoryItemHandler _itemHandler = itemHandler;

    public override Type ServiceImplementationType => typeof(ShopImportCategoriesTimerService);    

    protected override IImportService Create(ILogger logger, 
        string name,
        IShopItem shopModel,
        IShopImportSettings shopImportSettings,
        ILoaderService browserService)
    {
        var loadOptionsService = (shopImportSettings.GetService(nameof(CategoryLoadOptions))?.Value) 
            ?? throw new InvalidDataException($"CategoryLoadOptions not exists for {shopImportSettings.ShopName}");
        var categoryLoadOptions = JsonSerializer.Deserialize<CategoryLoadOptions>(loadOptionsService);
        
        var htmlSearchOptionsService = shopImportSettings.GetService(nameof(HtmlSearchFactoryOptions))?.Value;
        var htmlSearchFactoryOptions = htmlSearchOptionsService != null ? JsonSerializer.Deserialize<HtmlSearchFactoryOptions>(htmlSearchOptionsService) : null;

        var htmlSearcher = htmlSearchFactoryOptions != null 
            ? HtmlSearchFactory.CreateSearcher(htmlSearchFactoryOptions.SearchMatchType, htmlSearchFactoryOptions.SearchElementType)
            : null;

        return new ShopImportCategoriesTimerService(logger as ILogger<ShopImportCategoriesTimerService>, 
            name,
            htmlSearcher,
            browserService,
            shopModel as ICategoryShopModel,
            categoryLoadOptions, _itemHandler);
    }

    protected override ILogger GetLogger(ILogger logger, string name, IImportServiceLogFactory importServiceLogFactory, IShopItem shopModel, IShopImportSettings shopImportSettings)
    {
        if (logger is ILogger<ShopImportCategoriesTimerService> serviceLogger)
            return importServiceLogFactory?.GetLogger(serviceLogger, name, shopModel, shopImportSettings) ?? serviceLogger;
        else 
            throw new InvalidDataException(logger.GetType().FullName);
    }
}
