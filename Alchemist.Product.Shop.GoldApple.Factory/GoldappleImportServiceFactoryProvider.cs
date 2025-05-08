using DependencyInjection.ImplementationFactory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Threading;
using System.Text.Json;
using WebLoader.Common;
using Alchemist.Import.Factory;
using Alchemist.Product.Shop.GoldApple.ImportService;
using Alchemist.Product.Interfaces;

namespace Alchemist.Product.Shop.GoldApple.Factory;

public class GoldappleImportServiceFactoryProvider : IServiceImplementationFactory, IServiceImplementationFactory<GoldappleImportServiceFactory>
{
    private static GoldappleImportServiceFactory GetService(IServiceProvider serviceProvider, string key)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<GoldAppleImportService>>();

        var joinableTaskFactory = new JoinableTaskFactory(new JoinableTaskContext());
        var shopImportSettings = joinableTaskFactory.Run(async () =>
        {
            return await serviceProvider.GetAvailableSettingsWithHighestPriority(key, ShopSettingType.Product);
        }) ?? throw new InvalidDataException($"Shop settings {key} for type {ShopSettingType.Product} not found.");

        var browserDataLoader = serviceProvider.GetBrowserDataLoader(shopImportSettings.BrowserDataLoader.ImplementationTypeName);

        var webLoaderFactory = serviceProvider.GetWebLoaderFactory(shopImportSettings.WebLoader.ImplementationTypeName);

        if (webLoaderFactory == null)
            throw new InvalidDataException($"Webloader factory for {key} not found");

        var webLoader = webLoaderFactory.CreateWebLoader(browserDataLoader?.GetType().Name);

        var requestHeaders = JsonSerializer.Deserialize<RequestHeaders>(shopImportSettings.RequestHeaders.Value);

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
