using Alchemist.Import.Factory;
using Alchemist.Import.Html;
using Alchemist.Import.Html.Factory;
using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Interfaces;
using DependencyInjection.ImplementationFactory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Threading;
using System.Text.Json;
using WebLoader.Common;

namespace Alchemist.Import.Category.Json.Factory;

public class ShopImportCategoryJsonFactoryProvider : IServiceImplementationFactory
{
    private ShopImportCategoryJsonFactory GetService(IServiceProvider serviceProvider, string key)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<ShopImportCategoriesTimerService>>();       

        var joinableTaskFactory = new JoinableTaskFactory(new JoinableTaskContext());
        var shopImportSettings = joinableTaskFactory.Run(async () =>
        {
            return await serviceProvider.GetAvailableSettingsWithHighestPriority(key, ShopSettingType.Category);
        }) ?? throw new InvalidDataException($"Shop settings {key} for type {ShopSettingType.Category} not found.");

        var browserDataLoader = serviceProvider.GetBrowserDataLoader(shopImportSettings.BrowserDataLoader.ImplementationTypeName);

        var webLoaderFactory = serviceProvider.GetWebLoaderFactory(shopImportSettings.WebLoader.ImplementationTypeName);

        if (webLoaderFactory == null)
            throw new InvalidDataException($"Webloader factory for {key} not found");

        var webLoader = webLoaderFactory.CreateWebLoader(browserDataLoader?.GetType().Name);

        var requestHeaders = JsonSerializer.Deserialize<RequestHeaders>(shopImportSettings.RequestHeaders.Value);        

        var categoryLoadOptions = JsonSerializer.Deserialize<CategoryLoadOptions>(shopImportSettings.Services.OfType<IImportServiceSettings>().FirstOrDefault(s=>s.ServiceTypeName == nameof(CategoryLoadOptions))?.Value)
               ?? serviceProvider.GetKeyedService<CategoryLoadOptions>(key);

        var htmlSearchFactoryOptions = JsonSerializer.Deserialize<HtmlSearchFactoryOptions>(shopImportSettings.Services.OfType<IImportServiceSettings>().FirstOrDefault(s => s.ServiceTypeName == nameof(HtmlSearchFactoryOptions))?.Value)
               ?? serviceProvider.GetKeyedService<HtmlSearchFactoryOptions>(key);

        var htmlSearcher = htmlSearchFactoryOptions != null ? HtmlSearchFactory.CreateSearcher(htmlSearchFactoryOptions.SearchMatchType, htmlSearchFactoryOptions.SearchElementType)
          :  serviceProvider.GetKeyedService<IHtmlSearcher>(key)
                   ?? serviceProvider.GetServices<IHtmlSearcher>().FirstOrDefault(s => key == s.GetType().Name)
                   ?? serviceProvider.GetService<IHtmlSearcher>();

        return new ShopImportCategoryJsonFactory(logger, htmlSearcher, webLoader, requestHeaders, categoryLoadOptions);
    }

    object IServiceImplementationFactory.GetService(IServiceProvider serviceProvider, Type serviceType, object? key)
    {
        return GetService(serviceProvider, key?.ToString());
    }

    object IServiceImplementationFactory.GetService(IServiceProvider serviceProvider, Type serviceType)
    {
        return GetService(serviceProvider, nameof(ShopImportCategoriesTimerService));
    }
}
