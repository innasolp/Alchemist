namespace Alchemist.Product.Import.Model;

public interface IModelFactory
{
    IShopModel CreateShopModel(int shopId);

    IShopSettingTabsModel CreateShopSettingsTabsModel(int shopId, Guid shopGuid);

    IProductImportSettingsModel CreateProductImportSettingsModel(int shopId, Guid shopGuid);

    ICategoryImportSettingsModel CreateCategoryImportSettingsModel(int shopId, Guid shopGuid);

    IServiceSettingsModel CreateServiceSettingsModel(int shopId, int id, int parentId, Guid shopGuid, Guid shopSettingsGuid, string name);
}
