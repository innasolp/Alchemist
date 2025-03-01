using Alchemist.Product.Interfaces;

namespace Alchemist.Product.Import.Model;

public interface IImportFacade
{
    ShopImportModel? GetShopImport(Guid guid);

    bool TryGetShopImport(Guid guid, out ShopImportModel shopImport);

    Task<List<ShopImportModel>> LoadShops();

    ShopSettingsModel? GetShopSettings(Guid shopGuid, ShopSettingType shopSettingType);

    ServiceSettingsModel CreateServiceSettingsModel(Guid shopGuid, Guid shopSettingsGuid, string serviceSettingsName);
    
    ServiceSettingsModel? GetServiceSettingsModel(Guid shopGuid, Guid shopSettingsGuid, string serviceSettingsName);

    ShopImportModel AddNewShop(IShop shop);

    ShopSettingsModel CreateShopImportSettings(ShopSettingType shopSettingType, Guid shopGuid);

    Task<ShopSettingsModel?> GetShopImportSettings(ShopModel shop, ShopSettingType shopSettingType);

    Task Save(ShopSettingsModel shopSettingsModel);
}
