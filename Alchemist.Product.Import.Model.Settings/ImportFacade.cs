using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Import.Model.Infrastructure;
using Alchemist.Product.Interfaces;

namespace Alchemist.Product.Import.Model;

public class ImportFacade(IModelFactory modelFactory) : IImportFacade
{
    private readonly IModelFactory _modelFactory = modelFactory;

    private readonly Dictionary<Guid, ShopImportModel> _shopImports = [];

    public bool TryGetShopImport(Guid guid, out ShopImportModel shopImport)
    {
        return _shopImports.TryGetValue(guid, out shopImport) && shopImport != null;
    }    

    public bool TryGetShopSettings(Guid shopGuid, Alchemist.Import.Settings.Interfaces.ShopSettingType shopSettingType, out IShopImportSettingsModel shopSettings)
    {
        shopSettings = default;

        if (!_shopImports.TryGetValue(shopGuid, out var shopImportModel)
              || shopImportModel == null)
            return false;

        if (shopImportModel.ShopSettingTabs == null)
            return false;

        shopSettings = shopImportModel.ShopSettingTabs?.GetShopSettingsByType(shopSettingType);
        return shopSettings != null;
    }


    public bool TryGetShopSettings(Guid shopGuid, Guid shopSettingsGuid, out IShopImportSettingsModel shopSettings)
    {
        shopSettings = default;

        if (!_shopImports.TryGetValue(shopGuid, out var shopImportModel)
              || shopImportModel == null)
            return false;

        if (shopImportModel.ShopSettingTabs == null)
            return false;

        shopSettings = shopImportModel.ShopSettingTabs.GetShopSettingsByGuid(shopSettingsGuid);
        return shopSettings != null;
    }

    public bool TryGetShopSettings(Guid shopSettingsGuid, out IShopImportSettingsModel? shopSettings)
    {
       var shopSettingsTabs = _shopImports.Values.Where(s => s.ShopSettingTabs != null).Select(s => s.ShopSettingTabs);

        shopSettings = shopSettingsTabs.Where(t => t?.ShopProductsSettings?.Guid == shopSettingsGuid).Select(t=>t?.ShopProductsSettings).FirstOrDefault() as IShopImportSettingsModel
            ?? shopSettingsTabs.Where(t => t?.ShopCategoriesSettings?.Guid == shopSettingsGuid).Select(t => t?.ShopCategoriesSettings).FirstOrDefault();

        return shopSettings != null;
    }

    public bool TryGetServiceSettings(Guid shopGuid, Guid shopSettingsGuid, string serviceSettingsName, out IServiceSettingsModel serviceSettings)
    {
        serviceSettings = null;

        if (!TryGetShopSettings(shopGuid, shopSettingsGuid, out var shopSettings))
            return false;

        serviceSettings = shopSettings.GetServiceSettings(serviceSettingsName);
        return serviceSettings != null;
    }  
    
    public bool TryGetServiceSettings(Guid shopGuid, Guid shopSettingsGuid, Guid guid, out IServiceSettingsModel serviceSettings)
    {
        serviceSettings = null;

        if (!TryGetShopSettings(shopGuid, shopSettingsGuid, out var shopSettings))
            return false;

        serviceSettings = shopSettings.Services.OfType<IServiceSettingsModel>().FirstOrDefault(s => s.Guid == guid);
        return serviceSettings != null;
    }    

    public ShopImportModel AddNewShop(IShop shop)
    {
        var newShop = _modelFactory.CreateShopModel(shop.Id);
        newShop.SetFrom(shop);
        var newShopImport =  new ShopImportModel(newShop);

        newShopImport.ShopSettingTabs = _modelFactory.CreateShopSettingsTabsModel(newShop.Id, newShopImport.ShopGuid);
        newShopImport.ImportProducts = _modelFactory.CreateProductImportSettingsModel(newShop.Id, newShopImport.ShopGuid);
        newShopImport.ImportCategories = _modelFactory.CreateCategoryImportSettingsModel(newShop.Id, newShopImport.ShopGuid);

        _shopImports.Add(newShop.Guid, newShopImport);

        return newShopImport;
    }

    public async Task<List<ShopImportModel>> LoadShops(IEnumerable<IShop> shops)
    {        
        if (_shopImports.Count != 0)
        {
            shops.ToList().ForEach(s =>
            {
                var shopImport = _shopImports.FirstOrDefault(si => si.Value.Shop.Id == s.Id);
                if (shopImport.Value != null)
                    shopImport.Value.Shop.SetFrom(s);
                else
                    AddNewShop(s);
            });

            foreach (var deprecatedShop in _shopImports.Where(si => !shops.Any(s => s.Id == si.Value.Shop.Id)))
                deprecatedShop.Value.Shop.IsDeprecated = true;
        }
        else
            shops.ToList().ForEach(s => AddNewShop(s));

        return await Task.FromResult(_shopImports.Values.ToList());
    }

    public List<ShopImportModel> GetShops()
    {
        return _shopImports.Select(s => s.Value).ToList();
    }

    public void Reset()
    {
        _shopImports.Clear();
    }

    public bool TryGetTab(Guid shopGuid, TabType tab, out ITabModel tabSettings)
    {
        tabSettings = default;

        if (!_shopImports.TryGetValue(shopGuid, out var shopImportModel)
              || shopImportModel == null)
            return false;

        tabSettings = shopImportModel.GetTab(tab);

        return true;
    }    
    
    public ShopImportModel CreateDefaultShopImport()
    {
        var shopModel = _modelFactory.CreateShopModel(0);
        var shopImport = new ShopImportModel(shopModel)
        {
            ShopSettingTabs = _modelFactory.CreateShopSettingsTabsModel(0, shopModel.Guid),
            ImportProducts = _modelFactory.CreateProductImportSettingsModel(0, shopModel.Guid),
            ImportCategories = _modelFactory.CreateCategoryImportSettingsModel(0, shopModel.Guid)
        };

        return shopImport;
    }

    public void AddNewServiceSettings(IShopImportSettingsModel shopServicesSettingsModel, string serviceName, out IServiceSettingsModel serviceModel)
    {
       serviceModel = CreateNewServiceSettings(shopServicesSettingsModel, serviceName);

        shopServicesSettingsModel.Services.Add(serviceModel);
    }

    public IServiceSettingsModel CreateNewServiceSettings(IShopImportSettingsModel shopServicesSettingsModel, string serviceName)
    {
        var service =  _modelFactory.CreateServiceSettingsModel(shopId: (shopServicesSettingsModel as ISettings).ShopId,
            id: 0,
            parentId: shopServicesSettingsModel.Id,
            shopGuid: shopServicesSettingsModel.ShopGuid,
            shopSettingsGuid: shopServicesSettingsModel.Guid);
        service.Name = serviceName;
        return service;
    }
}
