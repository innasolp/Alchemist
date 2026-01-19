using ShopSettings.Interfaces;

namespace Alchemist.Product.ImportSettingsWebApp.Models;

public class PrimaryServiceModel(ShopImportSettingsModel parent)
{
    public string Name { get; set; }

    public ShopSettingType ShopSettingType { get; set; }

    public int ShopId {  get; set; }   

    public ServiceSettingsModel? ServiceSettings { get; set; }

    public ShopImportSettingsModel Parent { get; } = parent;

    public string ClassName { get; set; }
}
