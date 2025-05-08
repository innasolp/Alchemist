using Alchemist.Import.Settings.Interfaces;
using DependencyInjection.ImplementationFactory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Threading;
using System.Text.Json;
using WebLoader.Common;
using Alchemist.Import.Factory;
using Alchemist.Product.Shop.GoldApple.ImportService;

namespace Alchemist.Product.Shop.GoldApple.Factory;

public class GoldappleImportServiceFactoryProvider : IServiceImplementationFactory, IServiceImplementationFactory<GoldappleImportServiceFactory>
{
    private static GoldappleImportServiceFactory GetService(IServiceProvider serviceProvider, string key)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<GoldAppleImportService>>();

        var joinableTaskFactory = new JoinableTaskFactory(new JoinableTaskContext());
        var productShopImportSettings = joinableTaskFactory.Run(async () =>
        {
            return await serviceProvider.GetProductShopImportSettingsAsync<IProductShopImportSettings>(key);
        });

        var browserDataLoader = serviceProvider.GetBrowserDataLoader(productShopImportSettings.BrowserDataLoader.ImplementationTypeName);

        var webLoaderFactory = serviceProvider.GetWebLoaderFactory(productShopImportSettings.WebLoader.ImplementationTypeName);

        if (webLoaderFactory == null)
            throw new InvalidDataException($"Webloader factory for {key} not found");

        var webLoader = webLoaderFactory.CreateWebLoader(browserDataLoader?.GetType().Name);

        var requestHeaders = JsonSerializer.Deserialize<RequestHeaders>(productShopImportSettings.RequestHeaders.Value);

        return new GoldappleImportServiceFactory(webLoader, requestHeaders, logger);
    }

    object IServiceImplementationFactory.GetService(IServiceProvider serviceProvider, Type serviceType, object? key)
    {
        return GetService(serviceProvider, key?.ToString());
    }

    object IServiceImplementationFactory.GetService(IServiceProvider serviceProvider, Type serviceType)
    {
        return GetService(serviceProvider, nameof(GoldAppleImportService));
    }

    GoldappleImportServiceFactory IServiceImplementationFactory<GoldappleImportServiceFactory>.GetService(IServiceProvider serviceProvider, object? key)
    {
        return GetService(serviceProvider, key?.ToString());
    }

    GoldappleImportServiceFactory IServiceImplementationFactory<GoldappleImportServiceFactory>.GetService(IServiceProvider serviceProvider)
    {
        return GetService(serviceProvider, nameof(GoldAppleImportService));
    }
}
