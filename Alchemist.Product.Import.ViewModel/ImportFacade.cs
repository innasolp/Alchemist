using Alchemist.DataService.Interfaces;
using Alchemist.Import.Settings.Extensions;
using Alchemist.Product.Import.Model.Infrastructure;
using Alchemist.Product.Interfaces;

namespace Alchemist.Product.Import.Model;

public class ImportFacade(IShopDataService shopDataService) : IImportFacade
{
    private readonly IShopDataService _shopDataService = shopDataService;
    

    private readonly Dictionary<Guid, ShopImportModel> _shopImports = [];


    public bool TryGetShopImport(Guid guid, out ShopImportModel shopImport)
    {
        return _shopImports.TryGetValue(guid, out shopImport) && shopImport != null;
    }    

    public bool TryGetShopSettings(Guid shopGuid, ShopSettingType shopSettingType, out ShopSettingsModel shopSettings)
    {
        shopSettings = default;

        if (!_shopImports.TryGetValue(shopGuid, out var shopImportModel)
              || shopImportModel == null)
            return false;

        if (shopImportModel.ShopSettingTabs == null)
            return false;

        shopSettings = shopImportModel.ShopSettingTabs.GetShopSettingsByType(shopSettingType);
        return shopSettings != null;
    }

    public bool TryGetShopSettings(Guid shopGuid, Guid shopSettingsGuid, out ShopSettingsModel shopSettings)
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

    public bool TryGetServiceSettingsModel(Guid shopGuid, Guid shopSettingsGuid, string serviceSettingsName, out ServiceSettingsModel serviceSettings)
    {
        serviceSettings = null;

        if (!TryGetShopSettings(shopGuid, shopSettingsGuid, out var shopSettings))
            return false;

        serviceSettings = shopSettings.GetServiceSettings(serviceSettingsName);
        return serviceSettings != null;
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
            shops.ForEach(s =>
            {
                var shopImport = _shopImports.FirstOrDefault(si => si.Value.Shop.Id == s.Id);
                if (shopImport.Value != null)
                    shopImport.Value.Shop.Update(s);
                else
                    AddNewShop(s);
            });

            foreach (var deprecatedShop in _shopImports.Where(si => !shops.Any(s => s.Id == si.Value.Shop.Id)))
                deprecatedShop.Value.Shop.IsDeprecated = true;
        }
        else
            shops.ForEach(s => AddNewShop(s));

        return await Task.FromResult(_shopImports.Values.ToList());
    }
}
