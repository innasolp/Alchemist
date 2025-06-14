using Alchemist.Import.Settings.Interfaces;
using DependencyInjection.Interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Alchemist.Product.Import.Model.Infrastructure;

public static class ModelHelper
{
    public static SettingsModelBase? CreateDefaultTabModel(TabType tab)
    {
        return tab switch
        {
            TabType.Shop => new ShopSettingTabsModel(),
            TabType.Products => new ProductsImportSettingsModel(),
            TabType.Categories => new CategoriesImportSettingsModel(),
            _ => throw new InvalidOperationException($"No tab type with value {tab}"),
        };
    }

    public static ShopSettingsModel CreateShopSettings(Guid shopGuid, int shopId, ShopSettingType settingType)
    {
        return settingType == ShopSettingType.Product
           ? new ProductShopSettingsModel { ShopGuid = shopGuid, ShopId = shopId }
           : (settingType == ShopSettingType.Category ? new CategoryShopSettingsModel { ShopGuid = shopGuid, ShopId = shopId }
           : throw new InvalidOperationException($"{settingType}"));
    }

    public static ServiceSettingsModel CreateServiceSettingsModel(Guid shopGuid, int shopId, Guid shopSettingsGuid, string serviceSettingsName)
    {
        return new ServiceSettingsModel
        {
            ShopGuid = shopGuid,
            ShopSettingsGuid = shopSettingsGuid,
            ShopId = shopId,
            Name = serviceSettingsName
        };
    }

    public static SettingsModelBase? DeserializeWithNumberHandling(string json, Type type)
    {
        var option = new JsonSerializerOptions { NumberHandling = JsonNumberHandling.AllowReadingFromString, };
        return JsonSerializer.Deserialize(json, type, option) as SettingsModelBase;
    }

    public static ShopSettingsModel? GetShopSettingsFromJson(string json, ShopSettingType shopSettingType)
    {
        var option = new JsonSerializerOptions
        {
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver
            {
                Modifiers = { JsonExtensions.IgnorePropertiesForSerialize(typeof(ServiceSettingsModel),
                nameof(ServiceSettingsModel.Value)) }
            }
        };

        return shopSettingType == ShopSettingType.Product
            ? JsonSerializer.Deserialize<ProductShopSettingsModel>(json, option)
            : JsonSerializer.Deserialize<CategoryShopSettingsModel>(json, option);
    }

    public static async Task<ShopSettingsModel?> GetShopSettingsFromJsonAsync(Stream jsonStream, ShopSettingType shopSettingType)
    {
        var option = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver
            {
                Modifiers = { JsonExtensions.IgnorePropertiesForSerialize(typeof(ServiceSettingsModel),
                nameof(ServiceSettingsModel.Value)) }
            }
        };

        return shopSettingType == ShopSettingType.Product
            ? await JsonSerializer.DeserializeAsync<ProductShopSettingsModel>(jsonStream, option)
            : await JsonSerializer.DeserializeAsync<CategoryShopSettingsModel>(jsonStream, option);
    }

    public static bool IsServiceSettingsPrimary(string name)
    {
        return name == nameof(IShopImportSettings.ImportService)
            || name == nameof(IShopImportSettings.BrowserDataLoader)
            || name == nameof(IShopImportSettings.WebLoader)
            || name == nameof(IShopImportSettings.RequestHeaders);
    }
}
