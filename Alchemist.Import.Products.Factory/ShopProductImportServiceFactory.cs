using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Settings.Product;
using Import.Factory.Interfaces;
using Import.Factory.Service;
using Import.Interfaces;
using Import.Settings.Interfaces;
using Microsoft.Extensions.Logging;

namespace Alchemist.Import.Factory.Products;

public abstract class ShopProductImportServiceFactory(ILogger logger,
    ILoaderServiceFactory browserServiceFactory,
    IProductItemHandler itemHandler,
    IImportServiceLogFactory? logFactory = null) : ImportServiceFactory(logger, browserServiceFactory, logFactory)
{
    private readonly IProductItemHandler _itemHandler = itemHandler;


    protected override IImportService Create(ILogger logger, 
        string name,
        IImportSource shopModel, 
        IShopImportSettings shopImportSettings,
        ILoaderService browserService)
    {
        return Create(logger, name, shopModel as IProductShopModel, shopImportSettings, _itemHandler, browserService);
    }

    protected abstract IImportService Create(ILogger logger,
        string name,
        IProductShopModel shopModel,
        IShopImportSettings shopImportSettings,
        IProductItemHandler productItemHandler, 
        ILoaderService browserService);
}
