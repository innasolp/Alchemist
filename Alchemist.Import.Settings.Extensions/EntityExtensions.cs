using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Entities;
using Alchemist.Product.Interfaces;
using System.Runtime;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Alchemist.Import.Settings.Extensions;

public static class EntityExtensions
{
    public static T ToShopImportSettings<T>(this IShopSettings shopSettings, params JsonConverter[] jsonConverters)
        where T : IShopImportSettings
    {
        var option = new JsonSerializerOptions();
        jsonConverters.ToList().ForEach(option.Converters.Add);

        var model = JsonSerializer.Deserialize<T>(shopSettings.JsonValue.ToString());

        return model;
    }

    public static T? ToImportServiceSettings<T>(this IShopSettings shopSettings, params JsonConverter[] jsonConverters)
        where T : IServiceSettings //, IShopSettings
    {
        var option = new JsonSerializerOptions();
        jsonConverters.ToList().ForEach(option.Converters.Add);

        var model = shopSettings.JsonValue != null 
            ? JsonSerializer.Deserialize<T>(shopSettings.JsonValue.ToString(), option) 
            : default(T);
        return model;
    }

    public static IShopSettings ToEntity<T>(this T shopSettings, JsonSerializerOptions? options = null)
        where T : class, IShopSettings
    {
        var json = JsonSerializer.Serialize(shopSettings, options);

        return new ShopSettings
        {
            Id = shopSettings.Id,
            JsonValue = JsonSerializer.Deserialize<JsonObject>(json),
            ShopId = shopSettings.ShopId,
            Type = shopSettings.Type,
            Name = shopSettings.Name,
            ParentSettingsId = shopSettings.ParentSettingsId
        };
    }
}
