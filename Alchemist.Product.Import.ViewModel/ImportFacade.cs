using Alchemist.DataService.Interfaces;
using Alchemist.Import.Settings.Extensions;
using Alchemist.Product.Import.Model.Infrastructure;
using Alchemist.Product.Interfaces;

namespace Alchemist.Product.Import.Model;

public class ImportFacade(IShopDataService shopDataService) : IImportFacade
{
    private readonly IShopDataService _shopDataService = shopDataService;
    

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
           : null;
    }

    public bool TryGetShopSettings(Guid shopGuid, ShopSettingType shopSettingType, out ShopSettingsModel shopSettings)
    {
        shopSettings = default;

        if (!_shopImports.TryGetValue(shopGuid, out var shopImportModel)
              || shopImportModel == null || shopImportModel.ShopSettingTabs == null)
            return false;

        shopSettings = shopImportModel.ShopSettingTabs.GetShopSettingsByType(shopSettingType);
        return shopSettings != null;
    }

    public bool TryGetShopSettings(Guid shopGuid, Guid shopSettingsGuid, out ShopSettingsModel shopSettings)
    {
        shopSettings = default;

        if (!_shopImports.TryGetValue(shopGuid, out var shopImportModel)
              || shopImportModel == null || shopImportModel.ShopSettingTabs == null)
            return false;

        shopSettings = shopImportModel.ShopSettingTabs.GetShopSettingsByGuid(shopSettingsGuid);
        return shopSettings != null;
    }

    public ServiceSettingsModel CreateServiceSettingsModel(Guid shopGuid, Guid shopSettingsGuid, string serviceSettingsName)
    {
        return new ServiceSettingsModel
        {
            ShopGuid = shopGuid,
            ShopSettingsGuid = shopSettingsGuid,
            Name = serviceSettingsName
        };
    }

    public ServiceSettingsModel? GetServiceSettingsModel(Guid shopGuid, Guid shopSettingsGuid, string serviceSettingsName)
    {
        return _shopImports.TryGetValue(shopGuid, out var shopImportModel)
                && shopImportModel != null && shopImportModel.ShopSettingTabs != null
            ? shopImportModel.ShopSettingTabs.GetShopSettingsByGuid(shopSettingsGuid)?.GetServiceSettings(serviceSettingsName)
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
            shops.ForEach(s => AddNewShop(s));

        return await Task.FromResult(_shopImports.Values.ToList());
    }
}
