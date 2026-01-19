using ShopSettings.Interfaces;

namespace Alchemist.Product.Import.Background.Settings;

internal class ShopSettings : IShopSettings
{
    public int Id { get; set; }

    public int? ParentSettingsId { get; set; }
    public int ShopId { get; set; }
    public bool? IsActual { get; set; }
    public string JsonValue { get; set; }
    public ShopSettingType Type { get; set; }
    public string Name { get; set; }
}