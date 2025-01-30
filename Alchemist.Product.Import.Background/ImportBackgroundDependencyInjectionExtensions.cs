using Alchemist.Common;
using Alchemist.Product.Interfaces;
using Alchemist.Log.Serilog;
using Microsoft.Extensions.Configuration;
using Serilog;
using Alchemist.Product.Entities;
using Alchemist.Product.DataService.Interfaces;

namespace Alchemist.Product.Import.Background;

public static class ImportBackgroundDependencyInjectionExtensions
{
    public static async Task<bool> InitShopUrlModelAsync(this IShopUrlModel shopUrlModel, IShopDataService shopApiClient)
    {
        var shop = (await shopApiClient.GetShopByName(shopUrlModel.Name) ?? await shopApiClient.GetShopByUrl(shopUrlModel.Url))
            ?? await shopApiClient.CreateShop(new Shop { Name = shopUrlModel.Name, Url = shopUrlModel.Url });

        if (shop == null)
            return await Task.FromResult(false);

        shopUrlModel.ShopId = shop.Id;

        var shopUrl = await shopApiClient.GetShopUrl(shop.Id) ??
            (!string.IsNullOrEmpty(shopUrlModel.ProductUrl) && !string.IsNullOrEmpty(shopUrlModel.CategoryUrl)
               ? await shopApiClient.CreateShopUrl(shopUrlModel) : null);
        if (shopUrl != null)
        {
            shopUrlModel.ProductUrl = shopUrl.ProductUrl;
            shopUrlModel.CategoryUrl = shopUrl.CategoryUrl;
            shopUrlModel.PageProductCount = shopUrl.PageProductCount;
        }

        var shopCategories = await shopApiClient.GetShopCategories(shop.Id);
        shopCategories?.ForEach(shopUrlModel.Categories.Add);

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
