using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Alchemist.Import.Settings.Interfaces;

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
