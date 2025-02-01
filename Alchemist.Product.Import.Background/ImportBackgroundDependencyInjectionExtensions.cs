using Alchemist.Common;
using Alchemist.Log.Serilog;
using Microsoft.Extensions.Configuration;
using Serilog;
using Alchemist.Product.Entities;
using Alchemist.Product.DataService.Interfaces;
using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Shop.Interfaces;

namespace Alchemist.Product.Import.Background;

public static class ImportBackgroundDependencyInjectionExtensions
{
    public static async Task<bool> InitShopModelAsync(this IShopModel shopModel, IShopDataService shopApiClient)
    {
        var shop = (await shopApiClient.GetShopByName(shopModel.ShopName) ?? await shopApiClient.GetShopByUrl(shopModel.ShopUrl))
           ?? await shopApiClient.CreateShop(new Shop { Name = shopModel.ShopName, Url = shopModel.ShopUrl });

        if (shop == null)
            return await Task.FromResult(false);

        shopModel.ShopId = shop.Id;

        return await Task.FromResult(true);
    }
    public static async Task<bool> InitProductShopModelAsync(this IProductShopModel productShopModel, IShopDataService shopApiClient)
    {
        var result = await productShopModel.InitShopModelAsync(shopApiClient);

        if(!result)
            return await Task.FromResult(false);

        var shopUrl = await shopApiClient.GetShopUrl(productShopModel.ShopId) ??
            (!string.IsNullOrEmpty(productShopModel.ProductUrl) && !string.IsNullOrEmpty(productShopModel.CategoryUrl)
               ? await shopApiClient.CreateShopUrl(new ShopUrl { CategoryUrl = productShopModel.CategoryUrl, ShopId = productShopModel.ShopId, ProductUrl = productShopModel.ProductUrl }) : null);
        if (shopUrl != null)
        {
            productShopModel.ProductUrl = shopUrl.ProductUrl;
            productShopModel.CategoryUrl = shopUrl.CategoryUrl;
        }

        var shopCategories = await shopApiClient.GetShopCategories(productShopModel.ShopId);
        shopCategories?.ForEach(productShopModel.Categories.Add);

        return await Task.FromResult(true);
    }

    public static LoggerConfiguration AddShopsSerilogSourceContextConfigs(this AppSerilogBuilder appSerilogBuilder, ShopSettings[] shops, string logPath)
    {
        foreach (var shopSetting in shops)
        {
            appSerilogBuilder.AddSourceContextLogConfig(Utils.CombinePath(logPath, shopSetting.Id), shopSetting.Id);
        }
        return appSerilogBuilder.LoggerConfiguration;
    }

    public static LoggerConfiguration AddShopsSerilogPropertyConfigs(this AppSerilogBuilder appSerilogBuilder, ShopSettings[] shops, string logPath)
    {
        foreach (var shopSetting in shops)
        {
            appSerilogBuilder.AddClassNameLogConfig(Utils.CombinePath(logPath, shopSetting.Id), shopSetting.Id);
        }
        return appSerilogBuilder.LoggerConfiguration;
    }

    public static LoggerConfiguration AddShopsWebPerfomanceConfigs(this AppSerilogBuilder appSerilogBuilder, ShopSettings[] shops, string logPath)
    {
        foreach (var shopSetting in shops.Where(s => s.Perfomance == true))
        {
            appSerilogBuilder.AddPerfomanceCounter(shopSetting.Url, Http.RequestHandling.PerfomanceCounter.EventIds.Perfomance.Id, logPath, shopSetting.Id);
        }
        return appSerilogBuilder.LoggerConfiguration;
    }
}
