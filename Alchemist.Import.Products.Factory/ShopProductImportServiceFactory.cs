using Alchemist.Import.Factory.BrowserService;
using Alchemist.Import.Factory.Logging;
using Alchemist.Import.Interfaces;
using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Service.Factory.Abstractions;
using Alchemist.Import.Settings.Interfaces;
using Microsoft.Extensions.Logging;

namespace Alchemist.Import.Factory.Products;

public abstract class ShopProductImportServiceFactory(ILogger logger,
    ILoaderServiceFactory browserServiceFactory,
    IProductItemHandler itemHandler,
    IImportServiceLogFactory? logFactory = null) : ShopImportServiceFactory(logger, browserServiceFactory, logFactory)
{
    private readonly IProductItemHandler _itemHandler = itemHandler;


    protected override IImportService Create(ILogger logger, 
        IShopItem shopModel, 
        IShopImportSettings shopImportSettings,
        ILoaderService browserService)
    {
        return Create(logger, shopModel as IProductShopModel, shopImportSettings, _itemHandler, browserService);
    }

    protected abstract IImportService Create(ILogger logger,
        IProductShopModel shopModel,
        IShopImportSettings shopImportSettings,
        IProductItemHandler productItemHandler, 
        ILoaderService browserService);
}
