using Alchemist.Product.Entities;
using Alchemist.Product.Interfaces;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Alchemist.Product.Import.Model.Infrastructure;

public static class EntityExtensions
{
    public static ShopModel ToModel(this IShop shop)
    {
        return new ShopModel { Id = shop.Id, Name = shop.Name };
    }

    public static T ToShopSettingsModel<T>(this IShopSettings shopSettings)
        where T : ShopSettingsModel
    {
        var model = JsonSerializer.Deserialize<T>(shopSettings.JsonValue.ToString());

        model.Id = shopSettings.Id;
        model.ShopId = shopSettings.ShopId;
        model.Name = shopSettings.Name;

        return model;
    }

    public static ServiceSettingsModel ToServiceSettingsModel(this IShopSettings shopSettings)        
    {
        var model = JsonSerializer.Deserialize<ServiceSettingsModel>(shopSettings.JsonValue.ToString());

        model.Id = shopSettings.Id;        
        model.Name = shopSettings.Name;
        model.ParentSettingsId = shopSettings.ParentSettingsId;

        return model;
    }

    public static ShopSettings ToEntity<T>(this T shopSettings)
        where T: ShopSettingsModel
    {
        var option = new JsonSerializerOptions
        {
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver
            {
                Modifiers = { JsonExtensions.IgnorePropertiesForSerialize(typeof(T),
                nameof(ShopSettingsModel.ShopGuid),
                nameof(ShopSettingsModel.Guid),
                nameof(ShopSettingsModel.Tab),
                nameof(ShopSettingsModel.Id),
                nameof(ShopSettingsModel.ShopId),
                nameof(ShopSettingsModel.ShopSettingType),
                nameof(ShopSettingsModel.FileName),
                nameof(ShopSettingsModel.BrowserDataLoader),
                nameof(ShopSettingsModel.WebLoader),
                nameof(ShopSettingsModel.ImportService),
                nameof(ShopSettingsModel.RequestHeaders),
                nameof(ShopSettingsModel.Services)) }
            }
        };

        var json = JsonSerializer.Serialize(shopSettings, option);

        return new ShopSettings
        {
            Id = shopSettings.Id,
            JsonValue = JsonSerializer.Deserialize<JsonObject>(json),
            ShopId = shopSettings.ShopId,
            Type = shopSettings.ShopSettingType,
            Name = shopSettings.Name
        };
    }

    public static ShopSettings ToEntity(this ServiceSettingsModel serviceModel, int shopId)
    {
        var option = new JsonSerializerOptions
        {
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver
            {
                Modifiers = { JsonExtensions.IgnorePropertiesForSerialize(typeof(ServiceSettingsModel),
                nameof(ServiceSettingsModel.ShopGuid),
                nameof(ServiceSettingsModel.Guid),
                nameof(ServiceSettingsModel.ShopSettingsGuid),
                nameof(ServiceSettingsModel.ParentSettingsId),
                nameof(ServiceSettingsModel.ShopSettingType),
                nameof(ServiceSettingsModel.JsonValue),
                nameof(ServiceSettingsModel.FileName),
                nameof(ServiceSettingsModel.Id)) }
            }
        };

        return new ShopSettings
        {
            Type = ShopSettingType.Service,
            ShopId = shopId,
            Name = serviceModel.Name,
            ParentSettingsId = serviceModel.ParentSettingsId,
            Id = serviceModel.Id ?? 0,
            JsonValue = serviceModel.JsonValue ?? JsonSerializer.Deserialize<JsonObject>(JsonSerializer.Serialize(serviceModel, option))
        };
    }

    public static void Update(this ShopModel shopModel, IShop shop)
    {
        shopModel.Name = shop.Name;
    }
}
