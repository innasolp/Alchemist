using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Alchemist.Product.Import.WebApp.Models;

public static class ModelHelper
{
    public static object? DeserializeWithNumberHandling(string json, Type type)
    {
        var option = new JsonSerializerOptions
        {
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            PropertyNameCaseInsensitive = true,
            IgnoreReadOnlyProperties = false,
            IgnoreReadOnlyFields = true,
            RespectRequiredConstructorParameters = true,
            WriteIndented = true,
        };
        return JsonSerializer.Deserialize(json, type, option);
    }

    public static ShopSettingsModel? GetShopSettingsFromJson(string json, ShopSettingType shopSettingType)
    {
        var option = new JsonSerializerOptions
        {
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            IgnoreReadOnlyProperties = true,
            RespectRequiredConstructorParameters = true,
            PropertyNameCaseInsensitive = true,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver
            {
                Modifiers = { Common.JsonExtensions.IgnorePropertiesForSerialize(typeof(ServiceSettingsModel),
                nameof(ServiceSettingsModel.JsonValue)) }
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
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };

        return shopSettingType == ShopSettingType.Product
            ? await JsonSerializer.DeserializeAsync<ProductShopSettingsModel>(jsonStream, option)
            : await JsonSerializer.DeserializeAsync<CategoryShopSettingsModel>(jsonStream, option);
    }
}
