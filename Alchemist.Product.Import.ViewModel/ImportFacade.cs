using Alchemist.DataService.Interfaces;
using Alchemist.Import.Settings.Extensions;
using Alchemist.Product.Entities;
using Alchemist.Product.Import.Model.Infrastructure;
using Alchemist.Product.Interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

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
           : null;
    }    

    public ServiceSettingsModel CreateServiceSettingsModel(Guid shopGuid, Guid shopSettingsGuid, string serviceSettingsName)
    {
        return new ServiceSettingsModel
        {
            ShopGuid = shopGuid,
            ShopSettingsGuid = shopSettingsGuid,
            ServiceName = serviceSettingsName
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

    public async Task Save(ShopSettingsModel shopSettingsModel)
    {
        var option = new JsonSerializerOptions
        {
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver
            {
                Modifiers = { JsonExtensions.IgnorePropertiesForSerialize(typeof(ShopSettingsModel),
                    nameof(ShopSettingsModel.BrowserDataLoader),
                    nameof(ShopSettingsModel.WebLoader),
                    nameof(ShopSettingsModel.ImportService),
                    nameof(ShopSettingsModel.RequestHeaders),
                    nameof(ShopSettingsModel.Services)) }
            }
        };

        var json = JsonSerializer.Serialize(shopSettingsModel, option);
        var shopSettings = new ShopSettings {
            Id = shopSettingsModel.Id,
            JsonValue = json, 
            ShopId = shopSettingsModel.ShopId,
            Type = shopSettingsModel.ShopSettingType
        };

        var services = (from serviceModel in shopSettingsModel.Services
                        let service = new ShopSettings
                        {
                            Type = ShopSettingType.Service,
                            ShopId = shopSettingsModel.ShopId,
                            Name = shopSettingsModel.Name,
                            ParentSettingsId = shopSettings.Id,
                            Id = shopSettingsModel.Id
                        }
                        select service).ToList();
        await _shopSettingsDataService.SaveShopSettings(shopSettings, services);
    }

}
