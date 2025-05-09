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
using System.Xml.Linq;
using System;
using Microsoft.VisualStudio.Threading;
using Alchemist.Import.Interfaces;

namespace Alchemist.Product.Import.Background;

public static class ImportBackgroundDependencyInjectionExtensions
{
    [Obsolete]
    public static async Task<bool> InitShopModelAsync(this IShop shopModel, IShopDataService shopApiClient)
    {
        var shop = (await shopApiClient.GetShopByName(shopModel.Name) ?? await shopApiClient.GetShopByUrl(shopModel.Url))
           ?? await shopApiClient.CreateShop(new Shop { Name = shopModel.Name, Url = shopModel.Url });

        if (shop == null)
            return await Task.FromResult(false);

        shopModel.Id = shop.Id;

        return await Task.FromResult(true);
    }

    [Obsolete]
    public static async Task<bool> SetShopProductCategoriesAsync(this IProductShopModel productShopModel,
       IShopDataService shopApiClient)
    {   
        //todo
        //var shopCategories = await shopApiClient.GetShopCategories(productShopModel.Id);
        //shopCategories?.ForEach(productShopModel.Categories.Add);

        return await Task.FromResult(true);
    }

    public static async Task<IShopModel> CreateShopModelAsync(this IShopDataService shopDataService, IShopImportSettings shopImportSettings)
    {
        var shop = shopImportSettings.Id != 0
                ? await shopDataService.GetShop(shopImportSettings.Id)
                : await shopDataService.GetShopByName(shopImportSettings.Name) ?? await shopDataService.GetShopByUrl(shopImportSettings.Url);
        return shop != null
                ? await Task.FromResult(new ShopModel { Name = shop.Name, Url = shop.Url, Id = shop.Id })
                : await Task.FromResult(new ShopModel { Name = shopImportSettings.Name, Url = shopImportSettings.Url });
    }

    public static async Task<IProductShopModel> CreateProductShopModelAsync(this IShopDataService shopDataService, IProductShopImportSettings shopImportSettings)
    {
        var shopModel = await shopDataService.CreateShopModelAsync(shopImportSettings);
        var productShopModel = new ProductShopModel { Name = shopModel.Name, Url = shopModel.Url,
            ProductUrl = shopImportSettings.ProductUrl, 
            CategoryUrl = shopImportSettings.CategoryUrl };

        if (productShopModel.Id != 0)
        {
            var shopCategories = await shopDataService.GetShopCategories(productShopModel.Id);
            shopCategories?.ForEach(c => productShopModel.Categories.Add(new ProductShopCategoryModel { Category = c.Category, ItemId = c.ItemId }));
        }

        return productShopModel;
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
