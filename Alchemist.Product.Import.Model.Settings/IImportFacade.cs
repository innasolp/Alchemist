using Alchemist.Product.Import.Model;
using Alchemist.Product.Import.Model.Infrastructure;
using Alchemist.Product.Interfaces;

namespace Alchemist.Product.Model.ShopSettings;

public interface IImportFacade
{
    bool TryGetShopImport(Guid guid, out ShopImportModel shopImport);

    Task<List<ShopImportModel>> LoadShops(IEnumerable<IShop> shops);  

    bool TryGetShopSettings(Guid shopGuid, ShopSettingType shopSettingType, out IShopImportSettingsModel shopSettings);

    bool TryGetShopSettings(Guid shopGuid, Guid shopSettingsGuid, out IShopImportSettingsModel shopSettings);

    bool TryGetShopSettings(Guid shopSettingsGuid, out IShopImportSettingsModel? shopSettings);

    bool TryGetServiceSettings(Guid shopGuid, Guid shopSettingsGuid, string serviceSettingsName, out IServiceSettingsModel serviceSettings);

    bool TryGetServiceSettings(Guid shopGuid, Guid shopSettingsGuid, Guid guid, out IServiceSettingsModel serviceSettings);

    ShopImportModel AddNewShop(IShop shop);

    List<ShopImportModel> GetShops();    

    bool TryGetTab(Guid shopGuid, TabType tab, out ITabModel settings);

    void Reset();

    ShopImportModel CreateDefaultShopImport();

    void AddNewServiceSettings(IShopImportSettingsModel shopServicesSettingsModel, string serviceName, out IServiceSettingsModel serviceSettingsModel );

    IServiceSettingsModel CreateNewServiceSettings(IShopImportSettingsModel shopServicesSettingsModel);
}
