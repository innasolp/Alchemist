using Alchemist.Import.Category.Interfaces;
using Alchemist.Import.Html;
using Alchemist.Product.Interfaces;
using DependencyInjection.ImplementationFactory;
using Log.Interceptors.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WebLoader.Interfaces;

namespace Alchemist.Import.Category.Json;

public class ShopImportCategoryTimerServiceProvider : IServiceImplementationFactory, IServiceImplementationFactory<IShopCategoryImportService>
{
    private static ShopImportCategoriesTimerService GetCategoriesService(IServiceProvider serviceProvider, object? key)
    {
        var logger = serviceProvider.GetLogger<ShopImportCategoriesTimerService>(key);
        var htmlSearcher = serviceProvider.GetRequiredKeyedService<IHtmlSearcher>(key);
        var shopUrl = serviceProvider.GetRequiredKeyedService<IShopUrlModel>(key);
        var requestHeaders = serviceProvider.GetKeyedService<RequestHeaders>(key);
        var loadOptions = serviceProvider.GetRequiredKeyedService<CategoryLoadOptions>(key);
        var webLoader = serviceProvider.GetRequiredKeyedService<IWebLoader>(key);

        return new ShopImportCategoriesTimerService(logger, htmlSearcher, webLoader, shopUrl, requestHeaders, loadOptions);
    }

    private static ShopImportCategoriesTimerService GetCategoriesService(IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<ShopImportCategoriesTimerService>>();
        var htmlSearcher = serviceProvider.GetRequiredService<IHtmlSearcher>();
        var shopUrl = serviceProvider.GetRequiredService<IShopUrlModel>();
        var requestHeaders = serviceProvider.GetService<RequestHeaders>();
        var loadOptions = serviceProvider.GetRequiredService<CategoryLoadOptions>();
        var webLoader = serviceProvider.GetRequiredService<IWebLoader>();

        return new ShopImportCategoriesTimerService(logger, htmlSearcher, webLoader, shopUrl, requestHeaders, loadOptions);
    }

    public IShopCategoryImportService GetService(IServiceProvider serviceProvider, object? key)
    {
        return GetCategoriesService(serviceProvider, key);
    }

    public object GetService(IServiceProvider serviceProvider, Type serviceInterfaceType, object? key)
    {
        return GetCategoriesService(serviceProvider, key);
    }

    public IShopCategoryImportService GetService(IServiceProvider serviceProvider)
    {
        return GetCategoriesService(serviceProvider);
    }

    public object GetService(IServiceProvider serviceProvider, Type serviceInterfaceType)
    {
        return GetCategoriesService(serviceProvider);
    }
}
