using Alchemist.Product.Interfaces;

namespace Alchemist.Product.ImportSettingsWebApp.Models;

public class ShopImportSettingsTabModel
{
    public int? ShopId { get; set; } = null;

    public ShopSettingType ShopSettingType { get; set; } = ShopSettingType.Product;
}
