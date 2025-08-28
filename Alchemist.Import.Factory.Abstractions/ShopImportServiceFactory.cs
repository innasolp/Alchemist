using Alchemist.Import.Interfaces;
using Alchemist.Import.Service.Factory.Interfaces;
using Alchemist.Import.Settings.Interfaces;
using Alchemist.Import.Settings.Extensions;
using Http.RequestHandling.Interfaces;
using Http.RequestHandling.PerfomanceCounter;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using WebLoader.Common;
using WebLoader.Interfaces;
using Alchemist.Import.Factory.BrowserService;
using Alchemist.Import.Factory.Logging;

namespace Alchemist.Import.Service.Factory.Abstractions;

public abstract class ShopImportServiceFactory(ILogger logger,
    IEnumerable<IWebLoader> webLoaders,   
    IBrowserServiceFactory browserServiceFactory,   
    IImportServiceLogFactory? logFactory=null,
    IPerfomanceCounter? perfomanceCounter = null) : IShopImportServiceFactory
{
    private readonly ILogger _logger = logger;

    private readonly IEnumerable<IWebLoader> _webLoaders = webLoaders;

    private readonly IBrowserServiceFactory _browserServiceFactory = browserServiceFactory;

    private readonly IImportServiceLogFactory? _logFactory = logFactory;

    private readonly IPerfomanceCounter? _perfomanceCounter = perfomanceCounter;

    public abstract Type ServiceImplementationType { get; }

    IImportService IShopImportServiceFactory.Create(IShopItem shopModel, IShopImportSettings shopImportSettings)
    {
        var webLoaderSettings = shopImportSettings.GetWebLoader();
        var webLoader = _webLoaders.FirstOrDefault(f => f.GetType().Name == webLoaderSettings.ImplementationTypeName
            || f.GetType().Name.Contains(webLoaderSettings.ImplementationTypeName, StringComparison.InvariantCultureIgnoreCase))
            ?? throw new InvalidDataException($"Web loader of type {webLoaderSettings.ImplementationTypeName} not found");

        var browserDataLoaderSettings = shopImportSettings.GetBrowserDataLoader();
        var browserLauncherSettings = shopImportSettings.GetBrowserLauncher();
        var browserService = _browserServiceFactory.Create(browserDataLoaderSettings.ImplementationTypeName,
            browserLauncherSettings.ImplementationTypeName);

        var requstHeadersSettings = shopImportSettings.GetRequestHeaders();
        var requestHeaders = requstHeadersSettings == null ?
            null
            : JsonSerializer.Deserialize<RequestHeaders>(requstHeadersSettings.Value);

        if (_perfomanceCounter != null && shopImportSettings.Perfomance == true && webLoader is IRequestSender requestSender)
            _perfomanceCounter.Subscribe(requestSender);

        var logger = _logFactory == null ? _logger : GetLogger( _logger, _logFactory, shopModel, shopImportSettings) ?? _logger;

        return Create(logger, shopModel, shopImportSettings, webLoader, browserService, requestHeaders);
    }

    protected abstract ILogger GetLogger(ILogger logger, IImportServiceLogFactory importServiceLogFactory, IShopItem shopModel, IShopImportSettings shopImportSettings);

    protected abstract IImportService Create(ILogger logger, 
        IShopItem shopModel, 
        IShopImportSettings shopImportSettings,  
        IWebLoader webLoader, 
        ILoaderService browserService, 
        RequestHeaders? requestHeaders);
}
