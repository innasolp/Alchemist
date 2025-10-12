using Alchemist.Product.Interfaces;

namespace Alchemist.Product.ImportSettingsWebApp.Controllers;

public class ShopSettingsData
{
    public int? ShopId { get; set; }

    public int? ShopSettingsType { get; set; } = (int)ShopSettingType.Product;
}
