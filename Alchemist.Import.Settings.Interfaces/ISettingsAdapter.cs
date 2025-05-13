namespace Alchemist.Import.Settings.Interfaces;

public interface ISettingsAdapter
{
    Task<IShopImportSettings?> GetShopImportSettings(string shopSettingsName, ShopSettingType shopSettingType);

    Task<List<IShopImportSettings>> GetAllShopImportSettings();
}
