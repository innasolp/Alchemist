using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Alchemist.Product.Data;

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
public partial class ShopSettings 
{
    public int Id { get; set; }

    public int? ParentSettingsId { get; set; }
    public int ShopId { get; set; }
    public bool? IsActual { get; set; }
    public string JsonValue { get; set; }
    public ShopSettingType Type { get; set; }

    public string Name { get; set; }

    public DateTime AddedTs { get; set; }

    public DateTime? UpdatedTs { get; set; }
}
