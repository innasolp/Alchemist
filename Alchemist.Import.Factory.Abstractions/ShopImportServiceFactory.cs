using Alchemist.Import.Interfaces;
using Alchemist.Import.Service.Factory.Interfaces;
using Alchemist.Import.Settings.Interfaces;
using Microsoft.Extensions.Logging;
using Alchemist.Import.Factory.BrowserService;
using Alchemist.Import.Factory.Logging;

namespace Alchemist.Import.Service.Factory.Abstractions;

public abstract class ShopImportServiceFactory(ILogger logger,
    ILoaderServiceFactory browserServiceFactory,   
    IImportServiceLogFactory? logFactory=null) : IShopImportServiceFactory
{
    private readonly ILogger _logger = logger;

    private readonly ILoaderServiceFactory _browserServiceFactory = browserServiceFactory;

    private readonly IImportServiceLogFactory? _logFactory = logFactory;    

    public abstract Type ServiceImplementationType { get; }

    IImportService IShopImportServiceFactory.Create(string name, IShopItem shopModel, IShopImportSettings shopImportSettings)
    {
        var browserService = _browserServiceFactory.Create(name, shopImportSettings);   

        var logger = _logFactory == null ? _logger : GetLogger( _logger, name, _logFactory, shopModel, shopImportSettings) ?? _logger;

        return Create(logger, name, shopModel, shopImportSettings, browserService);
    }

    protected abstract ILogger GetLogger(ILogger logger, string name, IImportServiceLogFactory importServiceLogFactory, IShopItem shopModel, IShopImportSettings shopImportSettings);

    protected abstract IImportService Create(ILogger logger,
        string name,
        IShopItem shopModel, 
        IShopImportSettings shopImportSettings,  
        ILoaderService browserService);
}
