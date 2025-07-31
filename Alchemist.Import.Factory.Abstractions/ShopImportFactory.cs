using Alchemist.Import.Factory.Interfaces;
using Alchemist.Import.Interfaces;
using Alchemist.Import.Logging;
using Alchemist.Import.Settings.Interfaces;
using Http.RequestHandling.Interfaces;
using Http.RequestHandling.PerfomanceCounter;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using WebLoader.Common;
using WebLoader.Interfaces;

namespace Alchemist.Import.Factory.Abstractions;

public abstract class ShopImportFactory(ILogger logger,
    IEnumerable<IWebLoaderFactory> webLoaderFactories,   
    IImportServiceLogFactory? logFactory=null,
    IPerfomanceCounter? perfomanceCounter = null) : IShopImportServiceFactory
{
    private readonly ILogger _logger = logger;

    private readonly IEnumerable<IWebLoaderFactory> _webLoaderFactories = webLoaderFactories;

    private readonly IImportServiceLogFactory? _logFactory = logFactory;

    private readonly IPerfomanceCounter? _perfomanceCounter = perfomanceCounter;

    public abstract Type ServiceImplementationType { get; }

    IImportService IShopImportServiceFactory.Create(IShopItem shopModel, IShopImportSettings shopImportSettings)
    {
        var webLoaderFactory = _webLoaderFactories.FirstOrDefault(f => f.WebLoaderType.Name == shopImportSettings.WebLoader.ImplementationTypeName
            || f.WebLoaderType.Name.Contains(shopImportSettings.WebLoader.ImplementationTypeName, StringComparison.InvariantCultureIgnoreCase))
            ?? throw new InvalidDataException($"Web loader of type {shopImportSettings.WebLoader.ImplementationTypeName} not found");
        
        var webLoader = webLoaderFactory.CreateWebLoader(shopImportSettings.BrowserDataLoader?.ImplementationTypeName ?? "");

        var requestHeaders = shopImportSettings.RequestHeaders == null ?
            null
            : JsonSerializer.Deserialize<RequestHeaders>(shopImportSettings.RequestHeaders.Value);

        if (_perfomanceCounter != null && shopImportSettings.Perfomance == true && webLoader is IRequestSender requestSender)
            _perfomanceCounter.Subscribe(requestSender);

        var logger = _logFactory == null ? _logger : GetLogger( _logger, _logFactory, shopModel, shopImportSettings) ?? _logger;

        return Create(logger, shopModel, shopImportSettings, webLoader, requestHeaders);
    }

    protected abstract ILogger GetLogger(ILogger logger, IImportServiceLogFactory importServiceLogFactory, IShopItem shopModel, IShopImportSettings shopImportSettings);

    protected abstract IImportService Create(ILogger logger, 
        IShopItem shopModel, 
        IShopImportSettings shopImportSettings,  
        IWebLoader webLoader, 
        RequestHeaders? requestHeaders);
}
