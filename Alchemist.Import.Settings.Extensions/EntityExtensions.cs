using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Entities;
using Alchemist.Product.Interfaces;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Alchemist.Import.Settings.Extensions;

public static class EntityExtensions
{
    public static T ToShopImportSettings<T>(this IShopSettings shopSettings)
        where T : IShopImportSettings
    {
        var model = JsonSerializer.Deserialize<T>(shopSettings.JsonValue.ToString());

        model.Id = shopSettings.Id;
        model.ShopId = shopSettings.ShopId;
        model.Name = shopSettings.Name;

        return model;
    }

    public static T ToImportServiceSettings<T>(this IShopSettings shopSettings)
        where T : IImportServiceSettings
    {
        var model = JsonSerializer.Deserialize<T>(shopSettings.JsonValue.ToString());

        model.Id = shopSettings.Id;
        model.Name = shopSettings.Name;
        model.ParentSettingsId = shopSettings.ParentSettingsId;
        model.ShopId = shopSettings.ShopId;

        return model;
    }

    private static List<string> GetShopImportSettingsSerializeProperties()
    {
        return
        [
            nameof(IProductShopImportSettings.Url),
            nameof(IProductShopImportSettings.Caption),
            nameof(IProductShopImportSettings.Name),
            nameof(IProductShopImportSettings.Perfomance)
        ];
    }

    private static ShopSettings ToEntity<T>(this T shopSettings, ShopSettingType shopSettingType, JsonSerializerOptions options)
        where T : class, ISettings
    {
        var json = JsonSerializer.Serialize(shopSettings, options);

        return new ShopSettings
        {
            Id = shopSettings.Id,
            JsonValue = JsonSerializer.Deserialize<JsonObject>(json),
            ShopId = shopSettings.ShopId,
            Type = shopSettingType,
            Name = shopSettings.Name,
            ParentSettingsId = shopSettings.ParentSettingsId
        };
    }

    private static ShopSettings ProductSettingsToEntity<T>(this T shopSettings)
        where T : class, IProductShopImportSettings
    {
        var props = GetShopImportSettingsSerializeProperties();
        props.AddRange([nameof(IProductShopImportSettings.PageProductCount),
                nameof(IProductShopImportSettings.CategoryUrl),
                nameof(IProductShopImportSettings.ProductUrl) ]);

        var option = new JsonSerializerOptions
        {
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver
            {
                Modifiers = { JsonExtensions.SetPropertiesForSerialize(typeof(T), props.ToArray()) }
            }
        };

        return shopSettings.ToEntity(ShopSettingType.Product, option);
    }

    private static ShopSettings CategorySettingsToEntity<T>(this T shopSettings)
        where T : class, ICategoryShopImportSettings
    {
        var props = GetShopImportSettingsSerializeProperties();

        var option = new JsonSerializerOptions
        {
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver
            {
                Modifiers = { JsonExtensions.SetPropertiesForSerialize(typeof(T), props.ToArray()) }
            }
        };

        return shopSettings.ToEntity(ShopSettingType.Category, option);
    }

    private static ShopSettings ServiceSettingsToEntity<T>(this T service)
        where T : class, IImportServiceSettings
    {
        var option = new JsonSerializerOptions
        {
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver
            {
                Modifiers = { JsonExtensions.SetPropertiesForSerialize(typeof(T),
                nameof(IImportServiceSettings.AssemblyPath),
                nameof(IImportServiceSettings.ImplementationTypeName),
                nameof(IImportServiceSettings.ServiceTypeName),
                nameof(IImportServiceSettings.ServiceProviderPath),
                nameof(IImportServiceSettings.Value),
                nameof(IImportServiceSettings.Name)) }
            }
        };

        return service.ToEntity(ShopSettingType.Service, option);
    }

    public static ShopSettings? ToEntity<T>(this T shopSettings)
        where T : class, ISettings
    {
        switch (shopSettings.ShopSettingType)
        {
            case ShopSettingType.Product:
                return (shopSettings is IProductShopImportSettings productShopImportSettings) 
                    ? productShopImportSettings.ProductSettingsToEntity() 
                    : null;

            case ShopSettingType.Category:
                return (shopSettings is ICategoryShopImportSettings categoryShopImportSettings)
                    ? categoryShopImportSettings.CategorySettingsToEntity()
                    : null;

            case ShopSettingType.Service:
                return (shopSettings is IImportServiceSettings serviceSettings)
                    ? serviceSettings.ServiceSettingsToEntity()
                    : null;

            default:
                return null;
        }
    }
}
