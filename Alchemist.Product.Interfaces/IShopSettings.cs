namespace Alchemist.Product.Interfaces;

public enum ShopSettingType
{
    Service = 0,
    Product = 1,
    Category = 2
}

public interface IShopSettings
{
    int Id { get; set; }

    int? ParentSettingsId { get; set; }

    int ShopId { get; set; }

    bool? IsActual { get; set; }

    string JsonValue { get; set; }

    ShopSettingType Type { get; set; }
}
