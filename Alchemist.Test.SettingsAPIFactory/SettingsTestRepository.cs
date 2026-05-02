using Alchemist.Import.Settings.Extensions;
using Alchemist.Product.Data;
using System.Text.Json;
using SettingsCommon = Alchemist.Import.Settings.Extensions.Common;

namespace Alchemist.Test.SettingsAPIFactory;

public static class SettingsTestRepository
{
    public static Product.Data.ShopSettings CreateProductShopSettings(int shopId)
    {
        return CreateProductShopSettings(shopId, Guid.NewGuid().ToString());
    }

    public static Product.Data.ShopSettings CreateProductShopSettings(int shopId, string name)
    {
        var productShopImportSettings = new
        {
            ProductUrlFormat = $"https://product_url{Guid.NewGuid()}",
            CategoryUrlFormat = $"https://product_category_url{Guid.NewGuid()}"
        };
        var json = JsonSerializer.Serialize(productShopImportSettings);

        var shopSettings = new Product.Data.ShopSettings
        {
            ShopId = shopId,
            Type = ShopSettingType.Product,
            Name = name,
            JsonValue = json
        };
        return shopSettings;
    }

    public static Product.Data.ShopSettings CreateCategoryShopSettings(int shopId)
    {
        var categoryShopImportSettings = new { CategorySourceUrl = $"https://url{Guid.NewGuid()}" };
        var jsonValue = JsonSerializer.Serialize(categoryShopImportSettings);

        var productShopSettings = new Product.Data.ShopSettings
        {
            ShopId = shopId,
            Type = ShopSettingType.Category,
            Name = Guid.NewGuid().ToString(),
            JsonValue = jsonValue
        };
        return productShopSettings;
    }

    public static List<Product.Data.ShopSettings> CreateShopSettingsServicesTestData(Product.Data.ShopSettings shopSetting)
    {
        var serviceSettings = new List<Product.Data.ShopSettings>();
        var primaryServiceNames = SettingsCommon.GetPrimaryServiceNames().ToList();
        primaryServiceNames.RemoveAll(n => n == nameof(PrimaryServiceName.RequestHeaders));
        foreach (var primaryServiceName in primaryServiceNames)
        {
            var primaryService = CreateShopSettingsService(primaryServiceName);
            primaryService.ShopId = shopSetting.ShopId;
            primaryService.ParentSettingsId = shopSetting.Id;            
            serviceSettings.Add(primaryService);
        }

        return serviceSettings;
    }

    private static Product.Data.ShopSettings CreateShopSettingsService(string serviceName)
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
            Name = serviceName,
            Type = ShopSettingType.Service,
            JsonValue = jsonValue
        };

        return shopSetting;
    }
}