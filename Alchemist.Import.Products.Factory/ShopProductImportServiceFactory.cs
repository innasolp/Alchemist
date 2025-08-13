using Alchemist.Import.Factory.Abstractions;
using Alchemist.Import.Interfaces;
using Alchemist.Import.Logging;
using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Settings.Interfaces;
using Http.RequestHandling.PerfomanceCounter;
using Microsoft.Extensions.Logging;
using WebLoader.Common;
using WebLoader.Interfaces;

namespace Alchemist.Import.Products.Factory;

public abstract class ShopProductImportServiceFactory(ILogger logger,
    IEnumerable<IWebLoaderFactory> webLoaderFactories,
    IProductItemHandler itemHandler,
    IImportServiceLogFactory? logFactory = null,
    IPerfomanceCounter? perfomanceCounter = null) : ShopImportFactory(logger, webLoaderFactories, logFactory, perfomanceCounter)
{
    private readonly IProductItemHandler _itemHandler = itemHandler;    

    protected override IImportService Create(ILogger logger, IShopItem shopModel, IShopImportSettings shopImportSettings, IWebLoader webLoader, RequestHeaders? requestHeaders)
    {
        return Create(logger, shopModel as IProductShopModel, shopImportSettings, _itemHandler, webLoader, requestHeaders);
    }

    protected abstract IImportService Create(ILogger logger, IProductShopModel shopModel, IShopImportSettings shopImportSettings, IProductItemHandler productItemHandler, IWebLoader webLoader, RequestHeaders? requestHeaders);
}
