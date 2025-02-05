using Alchemist.Common;
using Alchemist.Product.Entities;
using Alchemist.Import.Categories.Data;
using Alchemist.Import.Category.Interfaces;
using Alchemist.Import.Html;
using Alchemist.Import.Products.Data;
using BrowserDataLoader.Interfaces;
using DependencyInjection.WorkerBuilder;
using Http.RequestHandling.Interfaces;
using Http.RequestHandling.PerfomanceCounter;
using Json.Extensions;
using Log.Interceptors.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog.Loggers;
using System.Reflection;
using WebLoader.Interfaces;
using Alchemist.Import.Products.Service;
using Message.SignalR.DependencyInjection;
using Grpc.Client.Extensions;
using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Shop.Interfaces;
using Alchemist.DataService.Interfaces;

namespace Alchemist.Product.Import.Background;

public class ShopImportWorkerBuilder(IHostApplicationBuilder builder) : WorkerBuilder(builder)
{
    private IServiceCollection AddSingletonForServiceByPath(Type serviceType, string servicePath, string appPath, out Type? implementationType)
    {
        var path = Utils.CombinePath(appPath, servicePath);

        implementationType = LoadImplementationType(path, serviceType);

        return implementationType != null ? Builder.Services.AddSingleton(serviceType, implementationType)
            : Builder.Services;
    }

    private IServiceCollection AddKeyedSingletonForServiceByPath(Type serviceType, string servicePath, string appPath, object? key, out Type? implementationType)
    {
        var path = Utils.CombinePath(appPath, servicePath);

        implementationType = LoadImplementationType(path, serviceType);

        return implementationType != null ? Builder.Services.AddKeyedSingleton(serviceType, key, implementationType)
            : Builder.Services;
    }

    private IServiceCollection AddServiceForShopSettings<TShopModelService>(ShopSettings shopSetting, string appPath, Type serviceInterfaceType, object? key, Func<ShopSettings, object> createShopModel)
        where TShopModelService:IShopModel
    {
        if (!string.IsNullOrEmpty(shopSetting.ServiceProviderPath))
        {
            var serviceProviderPath = Utils.CombinePath(appPath, shopSetting.ServiceProviderPath);
            AddServiceByKeyedImplementationFactory(serviceProviderPath, serviceInterfaceType, key);
        }
        else
        {
            var assemblyPath = Utils.CombinePath(appPath, shopSetting.Path);

            AddService(assemblyPath, serviceInterfaceType, out Type? serviceType);
        }

        Builder.Services.AddKeyedSingleton(typeof(TShopModelService), key, createShopModel(shopSetting));

        if (!string.IsNullOrEmpty(shopSetting.RequestHeadersPath))
            AddRequestHeaders(appPath, shopSetting.RequestHeadersPath, key);

        return Builder.Services;
    }

    public IServiceCollection AddProductServiceForShopSettings(ShopProductsSettings shopSetting, string appPath, Type serviceInterfaceType, object? key)
    {
        return AddServiceForShopSettings<IProductShopModel>(shopSetting, appPath, serviceInterfaceType, key,
             (shopSetting) =>
             {
                 return new ProductShopModel
                 {
                     ShopName = shopSetting.Name,
                     ShopUrl = shopSetting.Url,
                     ProductUrl = (shopSetting as ShopProductsSettings)?.ProductUrl,
                     CategoryUrl = (shopSetting as ShopProductsSettings)?.CategoryUrl,
                     PageProductCount = (shopSetting as ShopProductsSettings)?.PageProductCount
                 };
             });
    }

    private IServiceCollection AddRequestHeaders(string appPath, string requestHeadersPath, object? key)
    {
        var fullPath = Utils.CombinePath(appPath, requestHeadersPath);

        var requestHeaders = fullPath.ReadFromJsonFile<RequestHeaders>();

        return requestHeaders != null ?
            Builder.Services.AddKeyedSingleton(key, requestHeaders)
            : Builder.Services;
    }

    public IServiceCollection AddShopProducts(ShopProductsSettings[] shops, string appPath)
    {
        foreach (var shopSetting in shops)
        {
            if (shopSetting.BrowserDataLoader != null)
                AddSingletonForServiceByPath(typeof(IBrowserDataLoader), shopSetting.BrowserDataLoader.Path, appPath, out Type? browserDataLoaderType);

            AddKeyedSingletonForServiceByPath(typeof(IWebLoader), shopSetting.WebLoader.Path, appPath, shopSetting.Id, out Type? webLoaderType);

            AddProductServiceForShopSettings(shopSetting, appPath, typeof(IShopProductImportService), shopSetting.Id);

            if (shopSetting.Perfomance == true && webLoaderType != null && webLoaderType.GetInterfaces().Contains(typeof(IRequestSender)))
                Builder.Services.AddPerfomanceCounter(typeof(IWebLoader), (logger) => new SerilogUrlLogger(logger), shopSetting.Id);
        }

        return Builder.Services;
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

    private IServiceCollection AddCategoryServiceForShopSettings(ShopCategoriesSettings shopCategoriesSetting,
        string appPath,
        Type serviceInterfaceType,
        object? key)
    {
        var services = AddServiceForShopSettings<IShopModel>(shopCategoriesSetting, appPath, serviceInterfaceType, key, (shopSetting) =>
        {
            return new ShopModel
            {
                ShopName = shopSetting.Name,
                ShopUrl = shopSetting.Url
            };
        });

        if (shopCategoriesSetting.CategoryLoadOptions != null)
            AddKeyedServiceValueByTypeName(shopCategoriesSetting.CategoryLoadOptions, appPath, key);

        if (shopCategoriesSetting.HtmlSearchFactory != null)
            AddKeyedServiceValueByInterface(shopCategoriesSetting.HtmlSearchFactory, appPath, typeof(IHtmlSearchFactory), key);
        else
            Builder.Services.AddKeyedSingleton(typeof(IHtmlSearchFactory), key, new HtmlSearchFactory());

        if (shopCategoriesSetting.HtmlSearchSettings != null)
        {
            if (key != null)
                Builder.Services.AddKeyedSingleton(key, (serviceProvider, key) =>
                {
                    var htmlSearchFactory = serviceProvider.GetKeyedService<IHtmlSearchFactory>(key);
                    return htmlSearchFactory.CreateSearcher(shopCategoriesSetting.HtmlSearchSettings.SearchMatchType,
                        shopCategoriesSetting.HtmlSearchSettings.SearchElementType);
                });
            else
                Builder.Services.AddSingleton((serviceProvider) =>
                {
                    var htmlSearchFactory = serviceProvider.GetService<IHtmlSearchFactory>();
                    return htmlSearchFactory.CreateSearcher(shopCategoriesSetting.HtmlSearchSettings.SearchMatchType,
                        shopCategoriesSetting.HtmlSearchSettings.SearchElementType);
                });
        }


        return Builder.Services;
    }

    private IServiceCollection AddKeyedServiceValueFromAssembly(ServiceValueSettings serviceValueSettings, string appPath, Func<Assembly, Type?> getType, object? key)
    {
        var assemblyPath = Utils.CombinePath(appPath, serviceValueSettings.AssemblyPath);
        var valuePath = Utils.CombinePath(appPath, serviceValueSettings.ValuePath);

        return AddKeyedServiceValue(assemblyPath, valuePath, getType, key);
    }
    
    private IServiceCollection AddKeyedServiceValueByInterface(ServiceValueSettings serviceValueSettings, string appPath, Type interfaceType, object? key)
    {
        return AddKeyedServiceValueFromAssembly(serviceValueSettings, appPath, (assembly) => LoadServiceTypeByInterface(assembly, interfaceType), key);
    }

    private IServiceCollection AddKeyedServiceValueByTypeName(ServiceValueSettings serviceValueSettings, string appPath, object? key)
    {
        return AddKeyedServiceValueFromAssembly(serviceValueSettings, appPath, (assembly) => LoadServiceTypeByName(assembly, serviceValueSettings.TypeName), key);
    }    

    public IServiceCollection AddShopCategories(ShopCategoriesSettings[] shopCategories, string appPath)
    {
        foreach (var shopCategoriesSetting in shopCategories)
        {
            if (shopCategoriesSetting.BrowserDataLoader != null)
                AddSingletonForServiceByPath(typeof(IBrowserDataLoader), shopCategoriesSetting.BrowserDataLoader.Path, appPath, out Type? browserDataLoaderType);

            AddKeyedSingletonForServiceByPath(typeof(IWebLoader), shopCategoriesSetting.WebLoader.Path, appPath, shopCategoriesSetting.Id, out Type? webLoaderType);

            AddCategoryServiceForShopSettings(shopCategoriesSetting, appPath, typeof(IShopCategoryImportService), shopCategoriesSetting.Id);

            if (shopCategoriesSetting.Perfomance == true && webLoaderType != null && webLoaderType.GetInterfaces().Contains(typeof(IRequestSender)))
                Builder.Services.AddPerfomanceCounter(typeof(IWebLoader), (logger) => new SerilogUrlLogger(logger), shopCategoriesSetting.Id);
        }

        return Builder.Services;
    }

    public IServiceCollection AddPropertyValueInterceptorsLogging(ShopSettings[] shopSettings, string propertyName, Func<ShopSettings, object> getValue)
    {
        foreach (var shop in shopSettings)
        {
            Builder.Services.AddKeyedLogInterception(new SerilogPropertyKeyedLogInterceptor(propertyName, getValue(shop)), shop.Id);
        }

        return Builder.Services;
    }
}
