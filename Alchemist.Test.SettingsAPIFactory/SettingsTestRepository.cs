using Alchemist.Import.Settings.Extensions;
using Alchemist.Product.Interfaces;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using SettingsCommon = Alchemist.Import.Settings.Extensions.Common;
using ShopSettings = Alchemist.Product.Entities.ShopSettings;

namespace Alchemist.Test.SettingsAPIFactory;

public static class SettingsTestRepository
{
    public static IShopSettings CreateProductShopSettings(int shopId)
    {
        var productShopImportSettings = new
        {
            ProductUrlFormat = $"https://product_url{Guid.NewGuid()}",
            CategoryUrlFormat = $"https://product_category_url{Guid.NewGuid()}"
        };
        var json = JsonSerializer.Serialize(productShopImportSettings);
        var jsonValue = JsonSerializer.Deserialize<JsonObject>(json);

        var shopSettings = new ShopSettings
        {
            ShopId = shopId,
            Type = ShopSettingType.Product,
            Name = Guid.NewGuid().ToString(),
            JsonValue = jsonValue
        };
        return shopSettings;
    }

    public static IShopSettings CreateCategoryShopSettings(int shopId)
    {
        var categoryShopImportSettings = new { CategorySourceUrl = $"https://url{Guid.NewGuid()}" };
        var jsonValue = JsonSerializer.Deserialize<JsonObject>(JsonSerializer.Serialize(categoryShopImportSettings));

        var productShopSettings = new ShopSettings
        {
            ShopId = shopId,
            Type = ShopSettingType.Category,
            Name = Guid.NewGuid().ToString(),
            JsonValue = jsonValue
        };
        return productShopSettings;
    }

    public static List<IShopSettings> CreateShopSettingsServicesTestData(IShopSettings shopSetting)
    {
        var serviceSettings = new List<IShopSettings>();
        var primaryServiceNames = SettingsCommon.GetPrimaryServiceNames().ToList();
        primaryServiceNames.RemoveAll(n => n == nameof(PrimaryServiceName.RequestHeaders));
        foreach (var primaryServiceName in primaryServiceNames)
        {
            var primaryService = CreateShopSettingsService(primaryServiceName);
            primaryService.ShopId = shopSetting.ShopId;
            primaryService.ParentSettingsId = shopSetting.Id;            
            serviceSettings.Add(primaryService);
        }

        //var requestHeaders = new ShopSettings { Name = nameof(PrimaryServiceName.RequestHeaders) };
        //var fileName = "Ozon.Headers.Firefox.json";
        //var filePath = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/Content/{fileName}";

        //using var s = File.OpenRead(filePath);



        //requestHeaders.JsonValue = JsonSerializer.Deserialize<JsonObject>(s);
        //serviceSettings.Add(requestHeaders);

        return serviceSettings;
    }

    private static IShopSettings CreateShopSettingsService(string serviceName)
    {
        var serviceSettings = new
        {
            AssemblyPath = $"Folder{Guid.NewGuid()}",
            ImplementationTypeName = $"ServiceImplementation{Guid.NewGuid()}",
            ServiceTypeName = $"ServiceType{Guid.NewGuid()}"
        };

        var jsonValue = JsonSerializer.Deserialize<JsonObject>(JsonSerializer.Serialize(serviceSettings));

        var shopSetting = new ShopSettings 
        {            
            Name = serviceName,
            Type = ShopSettingType.Service,
            JsonValue = jsonValue
        };

        return shopSetting;
    }
}
