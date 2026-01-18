using Alchemist.Product.Entities;
using Alchemist.Product.Interfaces;
using Import.Settings.Interfaces;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Alchemist.Import.Settings.DataAdapter;

public static class EntityExtensions
{
    internal static JsonSerializerOptions GetDefaultServiceSerializationOptions<T>()
    {
        return new JsonSerializerOptions()
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver().WithAddedModifier(
            Common.JsonExtensions.IgnorePropertiesForSerialize(typeof(T),
                    nameof(IShopSettings.Id),
                    nameof(IShopSettings.ParentSettingsId),
                    nameof(IShopSettings.ShopId),
                    nameof(IShopSettings.Name)) 
            )
        };
    }

    internal static JsonSerializerOptions GetDefaultImportSettingsSerializationOptions<T>()
        where T : IShopImportSettings
    {
        return new JsonSerializerOptions()
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver().WithAddedModifier(
                Common.JsonExtensions.IgnorePropertiesForSerialize(typeof(T),
                    nameof(IShopImportSettings.Services),
                    nameof(IShopSettings.Id),
                    nameof(IShopSettings.ParentSettingsId),
                    nameof(IShopSettings.ShopId),
                    nameof(IShopSettings.Name)))
        };
    }

    internal static T ToShopImportSettings<T>(this IShopSettings shopSettings, params JsonConverter[] jsonConverters)
        where T : IShopImportSettings
    {
        var options = GetDefaultImportSettingsSerializationOptions<T>();
        jsonConverters.ToList().ForEach(options.Converters.Add);

        var model = JsonSerializer.Deserialize<T>(shopSettings.JsonValue.ToString(), options);

        return model;
    }

    public static T? ToImportServiceSettings<T>(this IShopSettings shopSettings, params JsonConverter[] jsonConverters)
        where T : IServiceSettings
    {
        var option = GetDefaultServiceSerializationOptions<T>();
        jsonConverters.ToList().ForEach(option.Converters.Add);

        var model = shopSettings.JsonValue != null 
            ? JsonSerializer.Deserialize<T>(shopSettings.JsonValue.ToString(), option) 
            : default;
        return model;
    }

    internal static IShopSettings ToEntity<T>(this T shopSettings, JsonSerializerOptions? options = null)
        where T : class, IShopSettings
    {
        options ??= GetDefaultServiceSerializationOptions<T>();

        var json = JsonSerializer.Serialize(shopSettings, options);

        return new ShopSettings
        {
            Id = shopSettings.Id,
            JsonValue = json,
            ShopId = shopSettings.ShopId,
            Type = shopSettings.Type,
            Name = shopSettings.Name,
            ParentSettingsId = shopSettings.ParentSettingsId
        };
    }
}
