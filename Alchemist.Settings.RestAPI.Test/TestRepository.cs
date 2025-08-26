using Alchemist.Import.Settings.Extensions;
using Alchemist.Product.Entities;
using Alchemist.Product.Interfaces;
using System.Text.Json;
using System.Text.Json.Nodes;

using SettingsCommon = Alchemist.Import.Settings.Extensions.Common;

namespace Alchemist.Settings.RestAPI.Test;

public static class TestRepository
{
    public static ShopSettings CreateCategoryShopSettings(int shopId, string shopName)
    {
        var shopSettings = new ShopSettings
        {
            ShopId = shopId,
            Type = ShopSettingType.Category,
            Name = $"{shopName}_category_{Guid.NewGuid()}"
        };

        ((IShopSettings)shopSettings).JsonValue = JsonSerializer.Serialize(new { CategorySourceUrl = $"https://category_{Guid.NewGuid()}" });

        return shopSettings;
    }

    public static ShopSettings CreateProductShopSettings(int shopId, string shopName)
    {
        var shopSettings = new ShopSettings
        {
            ShopId = shopId,
            Type = ShopSettingType.Category,
            Name = $"{shopName}_product_{Guid.NewGuid()}"
        };

        ((IShopSettings)shopSettings).JsonValue = JsonSerializer.Serialize(new
            {
                ProductUrlFormat = $"https://product_{Guid.NewGuid()}",
                CategoryUrlFormat = $"https://product_category_{Guid.NewGuid()}",
            }
        );

        return shopSettings;
    }

    public static List<ShopSettings> CreateShopSettingsServicesTestData(int shopId, int parentId)
    {
        var serviceSettings = new List<ShopSettings>();
        var primaryServiceNames = SettingsCommon.GetPrimaryServiceNames().ToList();
        primaryServiceNames.RemoveAll(n => n == nameof(PrimaryServiceName.RequestHeaders));
        foreach (var primaryServiceName in primaryServiceNames)
        {
            var primaryService = CreateShopSettingsService(primaryServiceName, shopId, parentId);
            serviceSettings.Add(primaryService);
        }

        return serviceSettings;
    }

    private static Product.Entities.ShopSettings CreateShopSettingsService(string serviceName, int shopId, int parentId)
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
            ShopId = shopId,
            ParentSettingsId = parentId,
            Name = serviceName,
            Type = ShopSettingType.Service,
            JsonValue = jsonValue
        };

        return shopSetting;
    }
}
