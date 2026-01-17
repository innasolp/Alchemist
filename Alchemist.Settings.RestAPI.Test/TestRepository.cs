using Alchemist.Import.Settings.Extensions;
using Alchemist.Product.Data;
using System.Text.Json;

using SettingsCommon = Alchemist.Import.Settings.Extensions.Common;

namespace Alchemist.Settings.RestAPI.Test;

public static class TestRepository
{
    public static Product.Data.ShopSettings CreateCategoryShopSettings(int shopId, string shopName)
    {
        var shopSettings = new Product.Data.ShopSettings
        {
            ShopId = shopId,
            Type = ShopSettingType.Category,
            Name = $"{shopName}_category"
        };

        shopSettings.JsonValue = JsonSerializer.Serialize(new { CategorySourceUrl = $"https://category_{shopName}" });

        return shopSettings;
    }

    public static Product.Data.ShopSettings CreateProductShopSettings(int shopId, string shopName)
    {
        var shopSettings = new Product.Data.ShopSettings
        {
            ShopId = shopId,
            Type = ShopSettingType.Category,
            Name = $"{shopName}_product"
        };

        shopSettings.JsonValue = JsonSerializer.Serialize(new
            {
                ProductUrlFormat = $"https://product_{Guid.NewGuid()}",
                CategoryUrlFormat = $"https://product_category_{shopName}",
            }
        );

        return shopSettings;
    }

    public static List<Product.Data.ShopSettings> CreateShopSettingsServicesTestData(int shopId, int parentId)
    {
        var serviceSettings = new List<Product.Data.ShopSettings>();
        var primaryServiceNames = SettingsCommon.GetPrimaryServiceNames().ToList();
        primaryServiceNames.RemoveAll(n => n == nameof(PrimaryServiceName.RequestHeaders));
        foreach (var primaryServiceName in primaryServiceNames)
        {
            var primaryService = CreateShopSettingsService(primaryServiceName, shopId, parentId);
            serviceSettings.Add(primaryService);
        }

        return serviceSettings;
    }

    private static Product.Data.ShopSettings CreateShopSettingsService(string serviceName, int shopId, int parentId)
    {
        var serviceSettings = new
        {
            AssemblyPath = $"Folder{Guid.NewGuid()}",
            ImplementationTypeName = $"ServiceImplementation{Guid.NewGuid()}",
            ServiceTypeName = $"ServiceType{Guid.NewGuid()}"
        };

        var jsonValue = JsonSerializer.Serialize(serviceSettings);

        var shopSetting = new Product.Data.ShopSettings
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
