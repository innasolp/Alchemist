using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Shop.Ozon.ImportService;
using DependencyInjection.ImplementationFactory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Threading;
using System.Text.Json;
using WebLoader.Common;
using Alchemist.Import.Factory;

namespace Alchemist.Product.Shop.Ozon.Factory;

public class OzonImportServiceFactoryProvider : IServiceImplementationFactory, IServiceImplementationFactory<OzonImportServiceFactory>
{
    private static OzonImportServiceFactory GetService(IServiceProvider serviceProvider, string key)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<OzonImportService>>();

        var joinableTaskFactory = new JoinableTaskFactory(new JoinableTaskContext());
        var productShopImportSettings = joinableTaskFactory.Run(async () =>
        {
            return await serviceProvider.GetProductShopImportSettingsAsync<IProductShopImportSettings>(key);
        });

        var browserDataLoader = serviceProvider.GetBrowserDataLoader(productShopImportSettings.BrowserDataLoader.ImplementationTypeName);

        if (browserDataLoader == null)
            throw new InvalidDataException($"Browser for {key} not found");

        var webLoaderFactory = serviceProvider.GetWebLoaderFactory(productShopImportSettings.WebLoader.ImplementationTypeName);

        if (webLoaderFactory == null)
            throw new InvalidDataException($"Webloader factory for {key} not found");

        var webLoader = webLoaderFactory.CreateWebLoader(browserDataLoader.GetType().Name);

        var requestHeaders = JsonSerializer.Deserialize<RequestHeaders>(productShopImportSettings.RequestHeaders.Value);

        return new OzonImportServiceFactory(webLoader, requestHeaders, logger);
    }

    object IServiceImplementationFactory.GetService(IServiceProvider serviceProvider, Type serviceType, object? key)
    {
        return GetService(serviceProvider, key?.ToString());
    }

    object IServiceImplementationFactory.GetService(IServiceProvider serviceProvider, Type serviceType)
    {
        return GetService(serviceProvider, nameof(OzonImportService));
    }

    OzonImportServiceFactory IServiceImplementationFactory<OzonImportServiceFactory>.GetService(IServiceProvider serviceProvider, object? key)
    {
        return GetService(serviceProvider, key?.ToString());
    }

    OzonImportServiceFactory IServiceImplementationFactory<OzonImportServiceFactory>.GetService(IServiceProvider serviceProvider)
    {
        return GetService(serviceProvider, nameof(OzonImportService));
    }
}
