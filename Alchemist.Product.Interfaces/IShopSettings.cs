using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Alchemist.Product.Interfaces;

[JsonConverter(typeof(JsonStringEnumConverter<ShopSettingType>))]
public enum ShopSettingType : short
{
    [Description("Service")]
    Service = 0,

    [Description("Products")]
    Product = 1,

    [Description("Categories")]
    Category = 2
}

public interface IShopSettings
{
    int Id { get; set; }

    string Name { get; set; }

    int? ParentSettingsId { get; set; }

    int ShopId { get; set; }

    bool? IsActual { get; set; }

    string JsonValue { get; set; }

    ShopSettingType Type { get; set; }
}
