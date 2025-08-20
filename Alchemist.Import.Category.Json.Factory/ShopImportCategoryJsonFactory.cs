using Alchemist.Import.BrowserService.Factory;
using Alchemist.Import.Category.Interfaces;
using Alchemist.Import.Html.Factory;
using Alchemist.Import.Interfaces;
using Alchemist.Import.Logging.Factory;
using Alchemist.Import.Service.Factory.Abstractions;
using Alchemist.Import.Settings.Interfaces;
using Http.RequestHandling.PerfomanceCounter;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using WebLoader.Common;
using WebLoader.Interfaces;

namespace Alchemist.Import.Category.Json.Factory;

public class ShopImportCategoryJsonFactory(ILogger<ShopImportCategoriesTimerService> logger,
    IEnumerable<IWebLoader> webLoaders,
    IBrowserServiceFactory browserServiceFactory,
    ICategoryItemHandler itemHandler, 
    IImportServiceLogFactory? logFactory = null,
    IPerfomanceCounter? perfomanceCounter = null) 
    : ShopImportServiceFactory(logger, webLoaders, browserServiceFactory, logFactory, perfomanceCounter)
{
    private readonly ICategoryItemHandler _itemHandler = itemHandler;

    public override Type ServiceImplementationType => typeof(ShopImportCategoriesTimerService);    

    protected override IImportService Create(ILogger logger, 
        IShopItem shopModel,
        IShopImportSettings shopImportSettings,
        IWebLoader webLoader,
        IBrowserService browserService,
        RequestHeaders requestHeaders)
    {
        var loadOptionsService = (shopImportSettings.Services.OfType<IImportServiceSettings>().FirstOrDefault(s => s.ServiceTypeName == nameof(CategoryLoadOptions))?.Value) 
            ?? throw new InvalidDataException($"CategoryLoadOptions not exists for {shopImportSettings.Name}");
        var categoryLoadOptions = JsonSerializer.Deserialize<CategoryLoadOptions>(loadOptionsService);

        var htmlSearchOptionsService = shopImportSettings.Services.OfType<IImportServiceSettings>().FirstOrDefault(s => s.ServiceTypeName == nameof(HtmlSearchFactoryOptions))?.Value;
        var htmlSearchFactoryOptions = htmlSearchOptionsService != null ? JsonSerializer.Deserialize<HtmlSearchFactoryOptions>(htmlSearchOptionsService) : null;

        var htmlSearcher = htmlSearchFactoryOptions != null 
            ? HtmlSearchFactory.CreateSearcher(htmlSearchFactoryOptions.SearchMatchType, htmlSearchFactoryOptions.SearchElementType)
            : null;

        return new ShopImportCategoriesTimerService(logger as ILogger<ShopImportCategoriesTimerService>, htmlSearcher, webLoader, browserService, shopModel as ICategoryShopModel, requestHeaders, categoryLoadOptions, _itemHandler);
    }

    protected override ILogger GetLogger(ILogger logger, IImportServiceLogFactory importServiceLogFactory, IShopItem shopModel, IShopImportSettings shopImportSettings)
    {
        if (logger is ILogger<ShopImportCategoriesTimerService> serviceLogger)
            return importServiceLogFactory?.GetLogger(serviceLogger, shopModel, shopImportSettings) ?? serviceLogger;
        else 
            throw new InvalidDataException(logger.GetType().FullName);
    }
}
