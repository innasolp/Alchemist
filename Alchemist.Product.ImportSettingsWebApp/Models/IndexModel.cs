using ShopSettings.Interfaces;

namespace Alchemist.Product.ImportSettingsWebApp.Models;

public class IndexModel
{    
    public int? ShopId { get; set; }

    public ShopSettingType ShopSettingType { get; set; } = ShopSettingType.Product;
}
