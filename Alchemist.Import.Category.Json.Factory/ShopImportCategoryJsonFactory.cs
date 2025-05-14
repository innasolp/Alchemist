using Alchemist.Import.Category.Interfaces;
using Alchemist.Import.Factory.Abstractions;
using Alchemist.Import.Html.Factory;
using Alchemist.Import.Interfaces;
using Alchemist.Import.Logging;
using Alchemist.Import.Settings.Interfaces;
using Http.RequestHandling.PerfomanceCounter;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using WebLoader.Common;
using WebLoader.Interfaces;

namespace Alchemist.Import.Category.Json.Factory;

public class ShopImportCategoryJsonFactory(ILogger<ShopImportCategoriesTimerService> logger,
    IEnumerable<IWebLoaderFactory> webLoaderFactories,
    ICategoryItemHandler itemHandler, 
    IImportServiceLogFactory? logFactory = null,
    IPerfomanceCounter? perfomanceCounter = null) 
    : ShopImportFactory(logger, webLoaderFactories, itemHandler, logFactory, perfomanceCounter)
{

    public override Type ServiceImplementationType => typeof(ShopImportCategoriesTimerService);    

    protected override IImportService Create(ILogger logger, 
        IShopModel shopModel,
        IShopImportSettings shopImportSettings,
        IWebLoader webLoader, 
        IItemHandler itemHandler,
        RequestHeaders requestHeaders)
    {
        var categoryLoadOptions = JsonSerializer.Deserialize<CategoryLoadOptions>(shopImportSettings.Services.OfType<IImportServiceSettings>().FirstOrDefault(s => s.ServiceTypeName == nameof(CategoryLoadOptions))?.Value);

        var htmlSearchFactoryOptions = JsonSerializer.Deserialize<HtmlSearchFactoryOptions>(shopImportSettings.Services.OfType<IImportServiceSettings>().FirstOrDefault(s => s.ServiceTypeName == nameof(HtmlSearchFactoryOptions))?.Value);

        var htmlSearcher = HtmlSearchFactory.CreateSearcher(htmlSearchFactoryOptions?.SearchMatchType, htmlSearchFactoryOptions?.SearchElementType);

        return new ShopImportCategoriesTimerService(logger as ILogger<ShopImportCategoriesTimerService>, htmlSearcher, webLoader, shopModel, requestHeaders, categoryLoadOptions, itemHandler as ICategoryItemHandler);
    }

    protected override ILogger GetLogger(ILogger logger, IImportServiceLogFactory importServiceLogFactory, IShopModel shopModel, IShopImportSettings shopImportSettings)
    {
        if (logger is ILogger<ShopImportCategoriesTimerService> serviceLogger)
            return importServiceLogFactory?.GetLogger(serviceLogger, shopModel, shopImportSettings) ?? serviceLogger;
        else 
            throw new InvalidDataException(logger.GetType().FullName);
    }
}
