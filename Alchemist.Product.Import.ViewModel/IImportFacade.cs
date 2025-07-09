using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Import.Model.Infrastructure;

namespace Alchemist.Product.Import.Model;

public interface IImportFacade
{
    bool TryGetShopImport(Guid guid, out ShopImportModel shopImport);

    Task<List<ShopImportModel>> LoadShops(IEnumerable<Interfaces.IShop> shops);  

    bool TryGetShopSettings(Guid shopGuid, ShopSettingType shopSettingType, out IShopServicesSettingsModel shopSettings);

    bool TryGetShopSettings(Guid shopGuid, Guid shopSettingsGuid, out IShopServicesSettingsModel shopSettings);

    bool TryGetShopSettings(Guid shopSettingsGuid, out IShopServicesSettingsModel? shopSettings);

    bool TryGetServiceSettings(Guid shopGuid, Guid shopSettingsGuid, string serviceSettingsName, out IServiceSettingsModel serviceSettings);

    bool TryGetServiceSettings(Guid shopGuid, Guid shopSettingsGuid, Guid guid, out IServiceSettingsModel serviceSettings);

    ShopImportModel AddNewShop(Interfaces.IShop shop);

    List<ShopImportModel> GetShops();    

    bool TryGetTab(Guid shopGuid, TabType tab, out ITabModel settings);

    void Reset();

    ShopImportModel CreateDefaultShopImport();

    void AddNewServiceSettings(IShopServicesSettingsModel shopServicesSettingsModel, string serviceName, out IServiceSettingsModel serviceSettingsModel );

    IServiceSettingsModel CreateNewServiceSettings(IShopServicesSettingsModel shopServicesSettingsModel, string serviceName);
}
