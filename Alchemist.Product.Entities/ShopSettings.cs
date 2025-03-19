using Alchemist.Product.Interfaces;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Alchemist.Product.Entities;

public class ShopSettings : IShopSettings
{
    public int Id { get; set; }

    public int? ParentSettingsId { get; set; }
    public int ShopId { get; set; }
    public bool? IsActual { get; set; }
    public JsonObject? JsonValue { get; set; }
    public ShopSettingType Type { get; set; }
    public string? Name { get; set; }
    string IShopSettings.JsonValue
    {
        get => JsonValue?.ToString();
        set
        {
            if (value != null) JsonValue = JsonSerializer.Deserialize<JsonObject>(value);
        }
    }
}
