using Alchemist.Common;
using Alchemist.Log.Serilog;
using Microsoft.Extensions.Configuration;
using Serilog;
using Alchemist.Product.Entities;
using Alchemist.Import.Products.Interfaces;
using Alchemist.DataService.Interfaces;
using Alchemist.Import.Settings.Interfaces;
using DependencyInjection.Interfaces;
using Alchemist.Product.Interfaces;

namespace Alchemist.Product.Import.Background;

public static class ImportBackgroundDependencyInjectionExtensions
{
    public static async Task<bool> InitShopModelAsync(this IShop shopModel, IShopDataService shopApiClient)
    {
        var shop = (await shopApiClient.GetShopByName(shopModel.Name) ?? await shopApiClient.GetShopByUrl(shopModel.Url))
           ?? await shopApiClient.CreateShop(new Shop { Name = shopModel.Name, Url = shopModel.Url });

        if (shop == null)
            return await Task.FromResult(false);

        shopModel.Id = shop.Id;

        return await Task.FromResult(true);
    }

    public static async Task<bool> SetShopProductCategoriesAsync(this IProductShopModel productShopModel,
       IShopDataService shopApiClient)
    {   
        var shopCategories = await shopApiClient.GetShopCategories(productShopModel.Id);
        shopCategories?.ForEach(productShopModel.Categories.Add);

        return await Task.FromResult(true);
    }

    public static LoggerConfiguration AddShopsSerilogSourceContextConfigs(this AppSerilogBuilder appSerilogBuilder, IEnumerable<IShopImportSettings> shops, string logPath)
    {
        foreach (var shopSetting in shops)
        {
            appSerilogBuilder.AddSourceContextLogConfig(Utils.CombinePath(logPath, shopSetting.Name), shopSetting.Name);
        }
        return appSerilogBuilder.LoggerConfiguration;
    }

    public static LoggerConfiguration AddShopsSerilogPropertyConfigs(this AppSerilogBuilder appSerilogBuilder, IEnumerable<IShopImportSettings> shops, string logPath)
    {
        foreach (var shopSetting in shops)
        {
            appSerilogBuilder.AddClassNameLogConfig(Utils.CombinePath(logPath, shopSetting.Name), shopSetting.Name);
        }
        return appSerilogBuilder.LoggerConfiguration;
    }

    public static LoggerConfiguration AddShopsWebPerfomanceConfigs(this AppSerilogBuilder appSerilogBuilder, IEnumerable<IShopImportSettings> shops, string logPath)
    {
        foreach (var shopSetting in shops.Where(s => s.Perfomance == true))
        {
            appSerilogBuilder.AddPerfomanceCounter(shopSetting.Url, Http.RequestHandling.PerfomanceCounter.EventIds.Perfomance.Id, logPath, shopSetting.Name);
        }
        return appSerilogBuilder.LoggerConfiguration;
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
}
