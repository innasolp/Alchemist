using Alchemist.Import.Settings.Model;
using Alchemist.Product.Entities;
using Alchemist.Product.Interfaces;
using System.Text.Json;
using System.Text.Json.Nodes;
using Alchemist.Import.Settings.Extensions;
using System.Reflection;

namespace Alchemist.Product.Import.WebApp.IntegrationTest;

public static class TestRepository
{
    public static List<Shop> GetShopsTestData(int count)
    {
        var shops = new List<Shop>();
        for (int i=0;i< count; i++)
        {
            var shop = new Shop { Name = $"TestShop{i + 1}", ShopUrl = $"https://testshop{i + 1}" };
            shops.Add(shop);
        }

        return shops;
    }

    public static List<IShopSettings> GetShopSettingsImportTestData(IEnumerable<IShop> shops)
    {
        var shopSettings = new List<IShopSettings>();

        foreach (var shop in shops)
        {
            var productShopSettings = new ShopSettings { ShopId = shop.Id, Type = ShopSettingType.Product };
            var productShopImportSettings = new ProductShopImportSettings() { Url = $"https://url{Guid.NewGuid()}" };
            productShopSettings.JsonValue = JsonSerializer.Deserialize<JsonObject>(JsonSerializer.Serialize(productShopImportSettings));
            shopSettings.Add(productShopSettings);

            var categoryShopSettings = new ShopSettings { ShopId = shop.Id, Type = ShopSettingType.Category };
            var categoryShopImportSettings = new CategoryShopImportSettings() { Url = $"https://url{Guid.NewGuid()}" };
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
            var importService = GetShopSettingsServices(shopSetting.ShopId, shopSetting.Id, nameof(ShopImportSettings.ImportService));
            serviceSettings.Add(importService);
            
            var browserDataLoader = GetShopSettingsServices(shopSetting.ShopId, shopSetting.Id, nameof(ShopImportSettings.BrowserDataLoader));
            serviceSettings.Add(browserDataLoader);
            
            var webLoader = GetShopSettingsServices(shopSetting.ShopId, shopSetting.Id, nameof(ShopImportSettings.WebLoader));
            serviceSettings.Add(webLoader);

            var requestHeaders = new ShopSettings { ShopId = shopSetting.ShopId, ParentSettingsId = shopSetting.Id, Name = nameof(ShopImportSettings.RequestHeaders) };
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

        var serviceSettings = shopSetting.ToImportServiceSettings<ImportServiceSettings>();
        serviceSettings.AssemblyPath = $"C:\\Folder{Guid.NewGuid()}";
        serviceSettings.ImplementationTypeName = $"ServiceImplementation{Guid.NewGuid()}";
        serviceSettings.ImplementationTypeName = $"ServiceType{Guid.NewGuid()}";

        shopSetting.JsonValue = JsonSerializer.Deserialize<JsonObject>(JsonSerializer.Serialize(serviceSettings));

        return shopSetting;
    }

    
}
