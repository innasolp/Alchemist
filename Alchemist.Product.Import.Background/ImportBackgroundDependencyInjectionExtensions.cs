using Alchemist.Common;
using Alchemist.Import.Products.Interfaces;
using Alchemist.DataService.Interfaces;
using Alchemist.Import.Settings.Interfaces;
using DependencyInjection.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Alchemist.DependencyInjection.Common;
using Message.SignalR.DependencyInjection;
using Grpc.Client.Extensions;

namespace Alchemist.Product.Import.Background;

public static class ImportBackgroundDependencyInjectionExtensions
{
    internal static async Task<ShopModel> CreateShopModelAsync(this IShopDataService shopDataService, IShopImportSettings shopImportSettings)
    {
        var shop = shopImportSettings.Id != 0
                ? await shopDataService.GetShop(shopImportSettings.Id)
                : await shopDataService.GetShopByName(shopImportSettings.Name) ?? await shopDataService.GetShopByUrl(shopImportSettings.Url);
        return shop != null
                ? await Task.FromResult(new ShopModel { Name = shop.Name, Url = shop.Url, Id = shop.Id })
                : await Task.FromResult(new ShopModel { Name = shopImportSettings.Name, Url = shopImportSettings.Url });
    }

    internal static async Task<IProductShopModel> CreateProductShopModelAsync(this IShopDataService shopDataService, IProductShopImportSettings shopImportSettings)
    {
        var shopModel = await shopDataService.CreateShopModelAsync(shopImportSettings);
        var productShopModel = new ProductShopModel { Name = shopModel.Name, Url = shopModel.Url, Id = shopModel.Id,
            ProductUrl = shopImportSettings.ProductUrl, 
            CategoryUrl = shopImportSettings.CategoryUrl };

        if (productShopModel.Id != 0)
        {
            var shopCategories = await shopDataService.GetShopCategories(productShopModel.Id);
            shopCategories?.ForEach(c => productShopModel.Categories.Add(new ProductShopCategoryModel { Category = c.Category, ItemId = c.ItemId }));
        }

        return productShopModel;
    }
        
    public static void SetAppPath(this IShopImportSettings shopImportSettings, string appPath)
    {
        shopImportSettings.BrowserDataLoader?.SetAppPath(appPath);
        shopImportSettings.WebLoader?.SetAppPath(appPath);
        shopImportSettings.ImportService.SetAppPath(appPath);
        shopImportSettings.RequestHeaders?.SetAppPath(appPath);

        foreach(var serviceSettings in shopImportSettings.Services.OfType<IImportServiceSettings>().Where(s=>!Alchemist.Import.Settings.Interfaces.Common.BaseServiceNames.Contains(s.Name)))
            serviceSettings.SetAppPath(appPath);
    }

    private static void SetAppPath(this IServiceSettings serviceSettings, string appPath)
    {
        if(!string.IsNullOrEmpty(serviceSettings.ServiceProviderPath))
            serviceSettings.ServiceProviderPath = Utils.CombinePath(appPath, serviceSettings.ServiceProviderPath);
        
        if(!string.IsNullOrEmpty(serviceSettings.AssemblyPath))
            serviceSettings.AssemblyPath = Utils.CombinePath(appPath, serviceSettings.AssemblyPath);        
        
    }

    public static IServiceCollection AddShopImportDataReceiver(this IServiceCollection services, IConfiguration configuration, string signalRUrlSectionName)
    {
        var signalRUrl = configuration.GetHostSectionValue(signalRUrlSectionName);

        return services.AddKeyedSignalRMessageReceiver(signalRUrl, ShopImportWorkerKeys.DataMessageReceiverKey);
    }

    public static IServiceCollection AddGrpcServiceClient<T>(this IServiceCollection services, IConfiguration configuration, string grpcApiSectionName)
        where T : class, IProductDataService
    {
        var grpcApiHost = configuration.GetSection(grpcApiSectionName).Get<string>()?.SetEnvironmentLocalHostIfNeed();
        services.AddGrpcChannelWithoutCertificateCheck(grpcApiHost);
        return services.AddSingleton<IProductDataService, T>();
    }

    public static IServiceCollection ConfigureDefaultHttps(this IServiceCollection services)
    {
        return services.ConfigureHttpClientDefaults(builder =>
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

    public static IServiceCollection AddHttpMessageDelegatingHandler<TMessageHandler>(this IServiceCollection services, string apiHost)
        where TMessageHandler : DelegatingHandler
    {
        services.AddSingleton<TMessageHandler>();        
        return services;
    }

    public static IServiceCollection AddShopImportMessageSender(this IServiceCollection services, IConfiguration configuration, string signalRUrlSectionName, object? key)
    {
        var signalRUrl = configuration.GetHostSectionValue(signalRUrlSectionName);

        return services.AddKeyedSignalRMessageSender(signalRUrl, key);
    }
}
