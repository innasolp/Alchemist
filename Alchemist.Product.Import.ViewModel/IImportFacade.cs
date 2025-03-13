using Alchemist.Product.Interfaces;

namespace Alchemist.Product.Import.Model;

public interface IImportFacade
{
    bool TryGetShopImport(Guid guid, out ShopImportModel shopImport);

    Task<List<ShopImportModel>> LoadShops();  

    bool TryGetShopSettings(Guid shopGuid, ShopSettingType shopSettingType, out ShopSettingsModel shopSettings);

    bool TryGetShopSettings(Guid shopGuid, Guid shopSettingsGuid, out ShopSettingsModel shopSettings);
    
    bool TryGetServiceSettingsModel(Guid shopGuid, Guid shopSettingsGuid, string serviceSettingsName, out ServiceSettingsModel serviceSettings);

    ShopImportModel AddNewShop(IShop shop);
}
