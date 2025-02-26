using Alchemist.DataService.Interfaces;
using Alchemist.Import.Settings.Extensions;
using Alchemist.Product.Import.Model.Infrastructure;
using Alchemist.Product.Interfaces;

namespace Alchemist.Product.Import.Model;

public class ImportFacade(IShopDataService shopDataService, IShopSettingsDataService shopSettingsDataService) : IImportFacade
{
    private readonly IShopDataService _shopDataService = shopDataService;

    private readonly IShopSettingsDataService _shopSettingsDataService = shopSettingsDataService;

    private readonly Dictionary<Guid, ShopImportModel> _shopImports = [];  
    
    public ShopImportModel? GetShopImport(Guid guid)
    {
        if (_shopImports.TryGetValue(guid, out var shopImport) && shopImport != null)
            return shopImport;

        return null;
    }
    public bool TryGetShopImport(Guid guid, out ShopImportModel shopImport)
    {
        return _shopImports.TryGetValue(guid, out shopImport) && shopImport != null;
    }

    public ShopSettingsModel CreateShopImportSettings(ShopSettingType shopSettingType, Guid shopGuid)
    {
        return shopSettingType == ShopSettingType.Product
            ? new ProductShopSettingsModel { ShopGuid = shopGuid }
            : new CategoryShopSettingsModel { ShopGuid = shopGuid };
    }

    public ShopSettingsModel? GetShopSettings(Guid shopGuid, ShopSettingType shopSettingType)
    {
        return _shopImports.TryGetValue(shopGuid, out var shopImportModel)
               && shopImportModel != null && shopImportModel.ShopSettingTabs != null
           ? shopImportModel.ShopSettingTabs.GetShopSettingsByType(shopSettingType)
              ?? CreateShopImportSettings(shopSettingType, shopGuid)
           : null;
    }

    public ServiceSettingsModel? GetServiceSettingsModel(Guid shopGuid, int shopSettingType, string serviceSettingsName)
    {
        return _shopImports.TryGetValue(shopGuid, out var shopImportModel)
                && shopImportModel != null && shopImportModel.ShopSettingTabs != null
            ? shopImportModel.ShopSettingTabs.GetShopSettingsByType((ShopSettingType)shopSettingType)?.GetServiceSettings(serviceSettingsName) ?? new ServiceSettingsModel { ShopGuid = shopGuid, ServiceName = serviceSettingsName }
            : null;
    }

    public ShopImportModel AddNewShop(IShop shop)
    {
        var newShop = shop.ToModel();
        var newShopImport = new ShopImportModel { ShopGuid = newShop.Guid, Shop = newShop };
        _shopImports.Add(newShop.Guid, newShopImport);
        return newShopImport;
    }

    public async Task<List<ShopImportModel>> LoadShops()
    {
        var shops = await _shopDataService.GetShops();

        if (_shopImports.Count != 0)
        {
            var shopsDict = shops.ToDictionary(s => s.Id, s => s);
            foreach (var sd in shopsDict)
            {
                var shopImport = _shopImports.FirstOrDefault(s => s.Value.Shop.Id == sd.Key).Value;
                shopImport?.Shop.Update(sd.Value);
            }

            var newShops = shops.Where(s => !_shopImports.Any(i => i.Value.Shop.Id == s.Id)).ToList();
            newShops.ForEach(s => AddNewShop(s));
        }
        else        
            shops.ForEach(s =>AddNewShop(s));

        return await Task.FromResult(_shopImports.Values.ToList());
    }

    private async Task<ProductShopSettingsModel> GetProductShopImportSettings(IShopSettings shopSettings)
    {
        return await shopSettings.GetShopImportSettings<ProductShopSettingsModel, ServiceSettingsModel>
            (_shopSettingsDataService.GetChildSettings);
    }
    
    private async Task<CategoryShopSettingsModel> GetCategoryShopImportSettings(IShopSettings shopSettings)
    {
        return await shopSettings.GetShopImportSettings<CategoryShopSettingsModel, ServiceSettingsModel>
            (_shopSettingsDataService.GetChildSettings);
    }

    public async Task<ShopSettingsModel?> GetShopImportSettings(ShopModel shop, ShopSettingType shopSettingType)
    {
        var shopSettings = await _shopSettingsDataService.GetShopSettings(shop.Id, shopSettingType);

        if (shopSettings == null) return null;

        ShopSettingsModel? shopSettingsModel = shopSettingType == ShopSettingType.Category
            ? (await GetCategoryShopImportSettings(shopSettings))
            : (await GetProductShopImportSettings(shopSettings));            

        shopSettingsModel.ShopGuid = shop.Guid;
        shopSettingsModel.Services.ForEach(s => s.ShopGuid = shop.Guid);

        return shopSettingsModel;
    }

}
