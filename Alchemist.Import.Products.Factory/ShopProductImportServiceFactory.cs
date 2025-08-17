using Alchemist.Import.Factory.Abstractions;
using Alchemist.Import.Interfaces;
using Alchemist.Import.Logging;
using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Settings.Interfaces;
using BrowserDataLoader.Interfaces;
using Http.RequestHandling.PerfomanceCounter;
using Microsoft.Extensions.Logging;
using WebLoader.Common;
using WebLoader.Interfaces;

namespace Alchemist.Import.Products.Factory;

public abstract class ShopProductImportServiceFactory(ILogger logger,
    IEnumerable<IWebLoader> webLoaders,
    IEnumerable<IBrowserDataLoader> browserDataLoaders,
    IProductItemHandler itemHandler,
    IImportServiceLogFactory? logFactory = null,
    IPerfomanceCounter? perfomanceCounter = null) : ShopImportFactory(logger, webLoaders, browserDataLoaders, logFactory, perfomanceCounter)
{
    private readonly IProductItemHandler _itemHandler = itemHandler;    

    protected override IImportService Create(ILogger logger, 
        IShopItem shopModel, 
        IShopImportSettings shopImportSettings,
        IWebLoader webLoader,
        IBrowserDataLoader browserDataLoader,
        RequestHeaders? requestHeaders)
    {
        return Create(logger, shopModel as IProductShopModel, shopImportSettings, _itemHandler, webLoader, browserDataLoader, requestHeaders);
    }

    protected abstract IImportService Create(ILogger logger,
        IProductShopModel shopModel,
        IShopImportSettings shopImportSettings,
        IProductItemHandler productItemHandler, 
        IWebLoader webLoader,
        IBrowserDataLoader browserDataLoader,
        RequestHeaders? requestHeaders);
}
