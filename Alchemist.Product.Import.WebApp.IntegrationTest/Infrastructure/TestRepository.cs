using Alchemist.Product.Entities;
using Alchemist.Product.Interfaces;
using System.Text.Json;
using System.Text.Json.Nodes;
using Alchemist.Import.Settings.Extensions;
using System.Reflection;
using Alchemist.Product.Import.WebApp.Models;
using Alchemist.Product.Import.Model;

namespace Alchemist.Product.Import.WebApp.IntegrationTest.Infrastructure;

public static class TestRepository
{
    public static List<Shop> GetShopsTestData(int count)
    {
        var shops = new List<Shop>();
        for (int i=0;i< count; i++)
        {
            var shop = new Shop { Name = $"TestShop{i + 1}", Url = $"https://testshop{i + 1}" };
            shops.Add(shop);
        }

        return shops;
    }

    public static List<IShopSettings> GetShopSettingsImportTestData(IEnumerable<IShop> shops)
    {
        var shopSettings = new List<IShopSettings>();

        foreach (var shop in shops)
        {
            var shopGuid = Guid.NewGuid();
            var productShopSettings = new ShopSettings { ShopId = shop.Id, Type = ShopSettingType.Product, Name = Guid.NewGuid().ToString() };
            var productShopImportSettings = new ProductShopSettingsModel(productShopSettings.ShopId, productShopSettings.Id, shopGuid)
                                { ProductUrlFormat = $"https://url{Guid.NewGuid()}" };
            productShopSettings.JsonValue = JsonSerializer.Deserialize<JsonObject>(JsonSerializer.Serialize(productShopImportSettings));
            shopSettings.Add(productShopSettings);

            var categoryShopSettings = new ShopSettings { ShopId = shop.Id, Type = ShopSettingType.Category, Name = Guid.NewGuid().ToString() };
            var categoryShopImportSettings = new CategoryShopSettingsModel(categoryShopSettings.ShopId, categoryShopSettings.Id, shopGuid)
                { CategorySourceUrl = $"https://url{Guid.NewGuid()}" };
            categoryShopSettings.JsonValue = JsonSerializer.Deserialize<JsonObject>(JsonSerializer.Serialize(categoryShopImportSettings));
            shopSettings.Add(categoryShopSettings);
        }

        return shopSettings;
    }

    public static List<IShopSettings> GetShopSettingsServicesTestData(IEnumerable<IShopSettings> shopSettings)
    {
        var serviceSettings = new List<IShopSettings>();
        foreach (var shopSetting in shopSettings)
        {
            var importService = GetShopSettingsServices(shopSetting.ShopId, shopSetting.Id, nameof(IShopServicesSettingsModel.ImportService));
            serviceSettings.Add(importService);
            
            var browserDataLoader = GetShopSettingsServices(shopSetting.ShopId, shopSetting.Id, nameof(IShopServicesSettingsModel.BrowserDataLoader));
            serviceSettings.Add(browserDataLoader);
            
            var webLoader = GetShopSettingsServices(shopSetting.ShopId, shopSetting.Id, nameof(IShopServicesSettingsModel.WebLoader));
            serviceSettings.Add(webLoader);

            var requestHeaders = new ShopSettings { ShopId = shopSetting.ShopId, ParentSettingsId = shopSetting.Id, Name = nameof(IShopServicesSettingsModel.RequestHeaders) };
            var fileName = "Ozon.Headers.Firefox.json";
            var filePath = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/Content/{fileName}";

            using var s = File.OpenRead(filePath);
            requestHeaders.JsonValue = JsonSerializer.Deserialize<JsonObject>(s);
            serviceSettings.Add(requestHeaders);
        }

        return serviceSettings;
    }    


    private static IShopSettings GetShopSettingsServices(int shopId, int parentId, string serviceName)
    {
        var shopSetting = new ShopSettings { ShopId = shopId, ParentSettingsId = parentId, Name = serviceName };

        var serviceSettings = shopSetting.ToImportServiceSettings<ServiceSettingsModel>()
            ?? new ServiceSettingsModel(shopId, 0, parentId, Guid.NewGuid(), Guid.NewGuid());
        serviceSettings.AssemblyPath = $"C:\\Folder{Guid.NewGuid()}";
        serviceSettings.ImplementationTypeName = $"ServiceImplementation{Guid.NewGuid()}";
        serviceSettings.ImplementationTypeName = $"ServiceType{Guid.NewGuid()}";

        shopSetting.JsonValue = JsonSerializer.Deserialize<JsonObject>(JsonSerializer.Serialize(serviceSettings));

        return shopSetting;
    }

    
}
