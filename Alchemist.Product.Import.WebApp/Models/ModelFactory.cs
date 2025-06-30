using Alchemist.Product.Import.Model;

namespace Alchemist.Product.Import.WebApp.Models;

public class ModelFactory : IModelFactory
{
    public ICategoryImportSettingsModel CreateCategoryImportSettingsModel(int shopId, Guid shopGuid)
    {
        return new CategoriesImportSettingsModel(shopId, shopGuid); 
    }

    public IShopModel CreateDefaultShopModel()
    {
        return new ShopModel(0);
    }

    public IProductImportSettingsModel CreateProductImportSettingsModel(int shopId, Guid shopGuid)
    {
        return new ProductsImportSettingsModel(shopId, shopGuid);
    }

    public IServiceSettingsModel CreateServiceSettingsModel(int shopId, int id, int parentId, Guid shopGuid, Guid shopSettingsGuid, string name)
    {
        return new ServiceSettingsModel(shopId, id, parentId, shopSettingsGuid, shopGuid, name);
    }

    public IShopModel CreateShopModel(int shopId)
    {
        return new ShopModel(shopId);
    }
    public IShopSettingTabsModel CreateShopSettingsTabsModel(int shopId, Guid shopGuid)
    {
        return new ShopSettingTabsModel(shopId, shopGuid);
    }
}
