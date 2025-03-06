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
using Microsoft.Extensions.Hosting;
using Serilog.Loggers;
using WebLoader.Interfaces;
using Alchemist.Import.Products.Service;
using Message.SignalR.DependencyInjection;
using Grpc.Client.Extensions;
using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Shop.Interfaces;
using Alchemist.Import.Settings.Interfaces;
using Alchemist.DataService.Interfaces;
using WebLoader.Common;
using Alchemist.Import.Category.Interfaces;

namespace Alchemist.Product.Import.Background;

public class ShopImportWorkerBuilder(IHostApplicationBuilder builder) : WorkerBuilder(builder)
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
                Builder.Services.AddPerfomanceCounter(typeof(IWebLoader), (logger) => new SerilogUrlLogger(logger), shopSetting.Name);
        }

        if (shopSetting.RequestHeaders != null)
            AddKeyedServiceBySettings(typeof(RequestHeaders), shopSetting.RequestHeaders, shopSetting.Name);        

        foreach (var serviceSettings in shopSetting.Services)
            AddKeyedServiceBySettings(serviceSettings, shopSetting.Name);

        return Builder.Services;
    }

    public IServiceCollection AddProductShopBySettings(IProductShopImportSettings productShopImportSettings)
    {
        AddShopDependenciesBySettings(productShopImportSettings);

        AddServiceBySettings(typeof(IShopProductImportService), productShopImportSettings.ImportService, productShopImportSettings.Name);

        return Builder.Services.AddKeyedSingleton(typeof(IProductShopModel), productShopImportSettings.Name,
            new ProductShopModel
            {
                ShopName = productShopImportSettings.Caption,
                ShopUrl = productShopImportSettings.Url,
                ProductUrl = productShopImportSettings?.ProductUrl,
                CategoryUrl = productShopImportSettings?.CategoryUrl,
                PageProductCount = productShopImportSettings?.PageProductCount
            });
    }

    public IServiceCollection AddShopProducts(IProductShopImportSettings[] productShopImportSettings)
    {
        foreach (var settings in productShopImportSettings)
        {
            AddProductShopBySettings(settings);
        }
        return Builder.Services;
    }

    public IServiceCollection AddShopCategories(IShopImportSettings[] categoryShopImportSettings)
    {
        foreach (var settings in categoryShopImportSettings)
        {
            AddCategoryShopBySettings(settings);
        }
        return Builder.Services;
    }

    public IServiceCollection AddCategoryShopBySettings(IShopImportSettings categoryShopImportSettings)
    {
        AddShopDependenciesBySettings(categoryShopImportSettings);

        AddServiceBySettings(typeof(IShopCategoryImportService), categoryShopImportSettings.ImportService, categoryShopImportSettings.Name);

        return Builder.Services.AddKeyedSingleton(typeof(IShopModel), categoryShopImportSettings.Name,
            new ShopModel
            {
                ShopName = categoryShopImportSettings.Caption,
                ShopUrl = categoryShopImportSettings.Url
            });
    }

    public IServiceCollection AddShopImportDataReceiver(string signalRUrlSectionName)
    {
        var signalRUrl = Builder.Configuration.GetSection(signalRUrlSectionName).Get<string>()?.SetEnvironmentLocalHostIfNeed();

        return Builder.Services.AddKeyedSignalRMessageReceiver(signalRUrl, ShopImportWorkerKeys.DataMessageReceiverKey);
    }

    public IServiceCollection AddShopImportMessageSender(string signalRUrlSectionName)
    {
        var signalRUrl = Builder.Configuration.GetSection(signalRUrlSectionName).Get<string>()?.SetEnvironmentLocalHostIfNeed();

        return Builder.Services.AddKeyedSignalRMessageSender(signalRUrl, ShopImportWorkerKeys.ShopsMessageSenderKey);
    }

    public IServiceCollection AddRestApiClient<T>(string restApiSectionName)
        where T : class, IShopDataService
    {
        var restApiHost = Builder.Configuration.GetSection(restApiSectionName).Get<string>()?.SetEnvironmentLocalHostIfNeed();
        AddHttpClient(restApiHost);

        Builder.Services.AddKeyedSingleton("ShopApiClient", restApiHost);
        return Builder.Services.AddSingleton<IShopDataService, T>();
    }

    public IServiceCollection AddGrpcServiceClient<T>(string grpcApiSectionName)
        where T : class, IProductDataService
    {
        var grpcApiHost = Builder.Configuration.GetSection(grpcApiSectionName).Get<string>()?.SetEnvironmentLocalHostIfNeed();
        Builder.Services.AddGrpcChannelWithoutCertificateCheck(grpcApiHost);
        return Builder.Services.AddSingleton<IProductDataService, T>();
    }

    public IServiceCollection AddProductsHandler()
    {
        return Builder.Services.AddProductDataHandler();
    }

    public IServiceCollection AddCategoriesHandler()
    {
        return Builder.Services.AddCategoriesDataHandler();
    }

    public IServiceCollection AddPropertyValueInterceptorsLogging(IShopImportSettings[] shopSettings, string propertyName, Func<IShopImportSettings, object> getValue)
    {
        foreach (var shop in shopSettings)
        {
            Builder.Services.AddKeyedLogInterception(new SerilogPropertyKeyedLogInterceptor(propertyName, getValue(shop)), shop.Id);
        }

        return Builder.Services;
    }

    public IServiceCollection ConfigureDefaultHttps()
    {
        return Builder.Services.ConfigureHttpClientDefaults(builder =>
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
        var httpClientBuilder = Builder.Services.AddHttpClient(name);
        HttpClientBuilders.Add(name, httpClientBuilder);
        return httpClientBuilder;
    }

    public IServiceCollection AddHttpMessageDelegatingHandler<TMessageHandler>(string apiHost)
        where TMessageHandler : DelegatingHandler
    {
        Builder.Services.AddSingleton<TMessageHandler>();

        if (HttpClientBuilders.TryGetValue(apiHost, out var httpClientBuilder))
            httpClientBuilder.AddHttpMessageHandler(serviceProvider => serviceProvider.GetRequiredService<TMessageHandler>());

        return Builder.Services;
    }
}
