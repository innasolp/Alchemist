using Alchemist.Common;
using Alchemist.Import.Categories.Data;
using Alchemist.Import.Products.Data;
using BrowserDataLoader.Interfaces;
using DependencyInjection.WorkerBuilder;
using Http.RequestHandling.Interfaces;
using Http.RequestHandling.PerfomanceCounter;
using Log.Interceptors.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Loggers;
using WebLoader.Interfaces;
using Alchemist.Import.Products.Service;
using Message.SignalR.DependencyInjection;
using Grpc.Client.Extensions;
using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Settings.Interfaces;
using Alchemist.DataService.Interfaces;
using WebLoader.Common;
using Alchemist.Import.Category.Interfaces;
using Alchemist.Product.Interfaces;

namespace Alchemist.Product.Import.Background;

public class ShopImportWorkerBuilder(IServiceCollection services) : WorkerBuilder(services)
{
    protected Dictionary<string, IHttpClientBuilder> HttpClientBuilders { get; } = [];

    public IServiceCollection AddShopDependenciesBySettings(IShopImportSettings shopSetting)
    {
        if (shopSetting.BrowserDataLoader != null)
            AddKeyedServiceBySettings(typeof(IBrowserDataLoader), shopSetting.BrowserDataLoader, shopSetting.Name);

        AddKeyedServiceBySettings(typeof(IWebLoader), shopSetting.WebLoader, shopSetting.Name);

        if (shopSetting.Perfomance == true)
        {
            var requestSenderType = shopSetting.WebLoader.AssemblyPath.GetServiceImplementationFromAssembly(typeof(IRequestSender));
            if (requestSenderType != null)
                Services.AddPerfomanceCounter(typeof(IWebLoader), (logger) => new SerilogUrlLogger(logger), shopSetting.Name);
        }

        if (shopSetting.RequestHeaders != null)
            AddKeyedServiceBySettings(typeof(RequestHeaders), shopSetting.RequestHeaders, shopSetting.Name);
        else
            Services.AddKeyedSingleton(typeof(RequestHeaders), shopSetting.Name, new RequestHeaders());

            foreach (var serviceSettings in shopSetting.Services.OfType<IImportServiceSettings>().Where(s => string.IsNullOrEmpty(s.Name)
            || !Alchemist.Import.Settings.Interfaces.Common.BaseServiceNames.Contains(s.Name)))
                AddKeyedServiceBySettings(serviceSettings, shopSetting.Name);

        return Services;
    }

    public IServiceCollection AddProductShopBySettings(IProductShopImportSettings productShopImportSettings)
    {
        AddShopDependenciesBySettings(productShopImportSettings);

        AddServiceBySettings(typeof(IShopProductImportService), productShopImportSettings.ImportService, productShopImportSettings.Name);

        return Services.AddKeyedSingleton(typeof(IProductShopModel), productShopImportSettings.Name,
            new ProductShopModel
            {
                Name = productShopImportSettings.Name,
                Url = productShopImportSettings.Url,
                ProductUrl = productShopImportSettings?.ProductUrl,
                CategoryUrl = productShopImportSettings?.CategoryUrl,
                PageProductCount = productShopImportSettings?.PageProductCount
            });
    }

    public IServiceCollection AddShopProducts(IEnumerable<IProductShopImportSettings> productShopImportSettings)
    {
        foreach (var settings in productShopImportSettings)
        {
            AddProductShopBySettings(settings);
        }
        return Services;
    }

    public IServiceCollection AddShopCategories(IEnumerable<ICategoryShopImportSettings> categoryShopImportSettings)
    {
        foreach (var settings in categoryShopImportSettings)
        {
            AddCategoryShopBySettings(settings);
        }
        return Services;
    }

    public IServiceCollection AddCategoryShopBySettings(ICategoryShopImportSettings categoryShopImportSettings)
    {
        AddShopDependenciesBySettings(categoryShopImportSettings);

        AddServiceBySettings(typeof(IShopCategoryImportService), categoryShopImportSettings.ImportService, categoryShopImportSettings.Name);

        return Services.AddKeyedSingleton(typeof(IShop), categoryShopImportSettings.Name,
            new ShopModel
            {
                Name = categoryShopImportSettings.Name,
                Url = categoryShopImportSettings.Url
            });
    }

    public IServiceCollection AddShopImportDataReceiver(IConfiguration configuration, string signalRUrlSectionName)
    {
        var signalRUrl = configuration.GetSection(signalRUrlSectionName).Get<string>()?.SetEnvironmentLocalHostIfNeed();

        return Services.AddKeyedSignalRMessageReceiver(signalRUrl, ShopImportWorkerKeys.DataMessageReceiverKey);
    }

    public IServiceCollection AddShopImportMessageSender(IConfiguration configuration, string signalRUrlSectionName)
    {
        var signalRUrl = configuration.GetSection(signalRUrlSectionName).Get<string>()?.SetEnvironmentLocalHostIfNeed();

        return Services.AddKeyedSignalRMessageSender(signalRUrl, ShopImportWorkerKeys.ShopsMessageSenderKey);
    }

    public IServiceCollection AddRestApiClient<T>(IConfiguration configuration, string restApiSectionName)
        where T : class, IShopDataService
    {
        var restApiHost = configuration.GetSection(restApiSectionName).Get<string>()?.SetEnvironmentLocalHostIfNeed();
        AddHttpClient(restApiHost);

        Services.AddKeyedSingleton("ShopApiClient", restApiHost);
        return Services.AddSingleton<IShopDataService, T>();
    }

    public IServiceCollection AddGrpcServiceClient<T>(IConfiguration configuration, string grpcApiSectionName)
        where T : class, IProductDataService
    {
        var grpcApiHost = configuration.GetSection(grpcApiSectionName).Get<string>()?.SetEnvironmentLocalHostIfNeed();
        Services.AddGrpcChannelWithoutCertificateCheck(grpcApiHost);
        return Services.AddSingleton<IProductDataService, T>();
    }

    public IServiceCollection AddProductsHandler()
    {
        return Services.AddProductDataHandler();
    }

    public IServiceCollection AddCategoriesHandler()
    {
        return Services.AddCategoriesDataHandler();
    }

    public IServiceCollection AddPropertyValueInterceptorsLogging(IShopImportSettings[] shopSettings, string propertyName, Func<IShopImportSettings, object> getValue)
    {
        foreach (var shop in shopSettings)
        {
            Services.AddKeyedLogInterception(new SerilogPropertyKeyedLogInterceptor(propertyName, getValue(shop)), shop.Id);
        }

        return Services;
    }

    public IServiceCollection ConfigureDefaultHttps()
    {
        return Services.ConfigureHttpClientDefaults(builder =>
        {
            builder.ConfigurePrimaryHttpMessageHandler(
                () => new HttpClientHandler()
                {
                    ServerCertificateCustomValidationCallback = (req, cert, chain, errors) =>
                    {
                        return true;
                    }
                });
        });
    }

    public IHttpClientBuilder AddHttpClient(string name)
    {
        var httpClientBuilder = Services.AddHttpClient(name);
        HttpClientBuilders.Add(name, httpClientBuilder);
        return httpClientBuilder;
    }

    public IServiceCollection AddHttpMessageDelegatingHandler<TMessageHandler>(string apiHost)
        where TMessageHandler : DelegatingHandler
    {
        Services.AddSingleton<TMessageHandler>();

        if (HttpClientBuilders.TryGetValue(apiHost, out var httpClientBuilder))
            httpClientBuilder.AddHttpMessageHandler(serviceProvider => serviceProvider.GetRequiredService<TMessageHandler>());

        return Services;
    }
}
