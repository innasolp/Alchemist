using Alchemist.Product.Interfaces;

namespace Alchemist.Import.Settings.Interfaces;

public interface ISettings
{
    int Id { get; set; }

    int ShopId { get; set; }

    string? Name { get; set; }

    int? ParentSettingsId { get; set; }

    ShopSettingType ShopSettingType { get; }
}
