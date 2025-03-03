using Alchemist.Product.Interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alchemist.Product.Import.Model.Infrastructure;

public static class ModelExtensions
{
    public static SettingsModelBase? GetSettings(this ShopImportModel shopImport, TabType tab, bool last = false)
    {
        switch (tab)
        {
            case TabType.Shop:
                {
                    return !last ? shopImport.ShopSettingTabs
                        : (shopImport.ShopSettingTabs?.SelectedSettingsTab == ShopSettingType.Product 
                            ? shopImport.ShopSettingTabs.ShopProductsSettings
                            : shopImport.ShopSettingTabs?.ShopCategoriesSettings);
                }

            case TabType.Products:
                {
                    return shopImport.ImportProducts;
                }

            case TabType.Categories:
                {
                    return shopImport.ImportCategories;
                }

            default:
                {
                    return null;
                }
        }
    }

    public static SettingsModelBase CreateSettings(this ShopImportModel shopImport, TabType tab)
    {
        switch (tab)
        {
            case TabType.Shop:
                {
                    return new ShopSettingTabsModel()
                    { 
                        ShopGuid = shopImport.ShopGuid,
                        ShopId = shopImport.Shop.Id 
                    };                    
                }

            case TabType.Products:
                {
                    return new ProductsImportSettingsModel() { ShopGuid = shopImport.ShopGuid, ShopId = shopImport.Shop.Id };
                }

            case TabType.Categories:
                {
                    return new CategoriesImportSettingsModel() { ShopGuid = shopImport.ShopGuid, ShopId = shopImport.Shop.Id };
                }

            default:
                {
                    return null;
                }
        }
    }

    public static ShopSettingsModel CreateShopSettings(this ShopSettingTabsModel shopSettingTabs, ShopSettingType settingType)
    {
        return settingType == ShopSettingType.Product
            ? new ProductShopSettingsModel
            {
                ShopGuid = shopSettingTabs.ShopGuid,
                ShopId = shopSettingTabs.ShopId
            }
            : (settingType == ShopSettingType.Category ? new CategoryShopSettingsModel
            {
                ShopGuid = shopSettingTabs.ShopGuid,
                ShopId = shopSettingTabs.ShopId
            } : 
            throw new InvalidOperationException($"{settingType}"));
    }

    public static bool SetSettings(this ShopImportModel shopImport, TabType tab, SettingsModelBase settings)
    {
        switch (tab)
        {
            case TabType.Shop:
                {
                    if (settings is ShopSettingTabsModel shopSettingsTabModel)
                    {
                        shopImport.ShopSettingTabs = shopSettingsTabModel;
                        shopImport.ShopSettingTabs.ShopGuid = shopImport.ShopGuid;
                        return true;
                    }
                    else
                    {
                        shopImport.ShopSettingTabs ??= new ShopSettingTabsModel() { ShopGuid = shopImport.ShopGuid, ShopId = shopImport.Shop.Id };
                        if (shopImport.ShopSettingTabs.SelectedSettingsTab == ShopSettingType.Product)
                        {
                            if (settings is not ProductShopSettingsModel productShopSettings)
                                return false;
                            shopImport.ShopSettingTabs.ShopProductsSettings = productShopSettings;
                            shopImport.ShopSettingTabs.ShopProductsSettings.ShopGuid = shopImport.ShopGuid;
                            return true;
                        }
                        else
                        {
                            if (settings is not CategoryShopSettingsModel categoryShopSettings)
                                return false;
                            shopImport.ShopSettingTabs.ShopCategoriesSettings = categoryShopSettings;
                            shopImport.ShopSettingTabs.ShopCategoriesSettings.ShopGuid = shopImport.ShopGuid;
                            return true;
                        }
                    }
                }

            case TabType.Products:
                {
                    if (settings is not ProductsImportSettingsModel productsImportSettings)
                        return false;
                    shopImport.ImportProducts = productsImportSettings;
                    shopImport.ImportProducts.ShopGuid = shopImport.ShopGuid;
                    return true;
                }

            case TabType.Categories:
                {
                    if (settings is not CategoriesImportSettingsModel categoriesImportSettings)
                        return false;
                    shopImport.ImportCategories = categoriesImportSettings;
                    shopImport.ImportCategories.ShopGuid = shopImport.ShopGuid;
                    return true;
                }

            default:
                {
                    return false;
                }
        }
    }
  
    public static ServiceSettingsModel? GetServiceSettings(this ShopSettingsModel shopSettings, string serviceName)
    {
        switch (serviceName)
        {
            case nameof(ShopSettingsModel.ImportService):
                return shopSettings.ImportService;

            case nameof(ShopSettingsModel.WebLoader):
                return shopSettings.WebLoader;

            case nameof(ShopSettingsModel.BrowserDataLoader):
                return shopSettings.BrowserDataLoader;

            default:
                return null;
        }
    }

    public static ShopSettingsModel? GetShopSettingsByType(this ShopSettingTabsModel shopSettingTabs, ShopSettingType shopSettingType)
    {
        if (shopSettingType == ShopSettingType.Service)
            throw new InvalidOperationException("Service tab not available for shop settings.");

        switch (shopSettingType)
        {
            case ShopSettingType.Product:
                {
                    return shopSettingTabs.ShopProductsSettings;
                }

            case ShopSettingType.Category:
                {
                    return shopSettingTabs.ShopCategoriesSettings;
                }

            default:
                return null;
        }
    }

    public static ShopSettingsModel? GetShopSettingsByGuid(this ShopSettingTabsModel shopSettingTabs, Guid shopSettingsGuid)
    {
        return shopSettingTabs?.ShopProductsSettings?.Guid == shopSettingsGuid
            ? shopSettingTabs.ShopProductsSettings
            : (shopSettingTabs?.ShopCategoriesSettings?.Guid == shopSettingsGuid ? shopSettingTabs.ShopCategoriesSettings : null);
    }
   
    public static bool UpdateServiceSettings(this ShopSettingsModel shopSettings, ServiceSettingsModel? serviceSettings)
    {
        if (serviceSettings == null) return false;
        switch (serviceSettings?.Name)
        {
            case nameof(ShopSettingsModel.ImportService):
                {
                    shopSettings.UpdateShopServiceSettings(nameof(ShopSettingsModel.ImportService),serviceSettings,  s => s.ImportService, (s, sm) => s.ImportService = sm);

                    return true;
                }

            case nameof(ShopSettingsModel.WebLoader):
                {
                    shopSettings.UpdateShopServiceSettings(nameof(ShopSettingsModel.WebLoader), serviceSettings, s => s.WebLoader, (s, sm) => s.WebLoader = sm);
                    return true;
                }

            case nameof(ShopSettingsModel.BrowserDataLoader):
                {
                    shopSettings.UpdateShopServiceSettings(nameof(ShopSettingsModel.BrowserDataLoader), serviceSettings, s => s.BrowserDataLoader, (s, sm) => s.BrowserDataLoader = sm);

                    return true;
                }

            default:
                {
                    shopSettings.Services.Add(serviceSettings);
                    return true;
                }
        }
    }

    private static void UpdateShopServiceSettings(this ShopSettingsModel shopSettings, string serviceName, ServiceSettingsModel? serviceSettings,
        Func<ShopSettingsModel, ServiceSettingsModel> get,
        Action<ShopSettingsModel,ServiceSettingsModel> set)
    {
        if (get(shopSettings) == null)
            set(shopSettings, new ServiceSettingsModel
            {
                ShopGuid = shopSettings.ShopGuid,
                ShopSettingsGuid = shopSettings.Guid,
                ParentSettingsId = shopSettings.Id,
                Id = serviceSettings.Id,
                Name = serviceName
            });
        get(shopSettings).Update(serviceSettings);

        if (!shopSettings.Services.Any(s => s.Guid == serviceSettings?.Guid))
            shopSettings.Services.Add(get(shopSettings));
    }       
    
    public static SettingsModelBase? DeserializeWithNumberHandling(this string json, Type type)       
    {
        var option = new JsonSerializerOptions { NumberHandling = JsonNumberHandling.AllowReadingFromString, };
        return JsonSerializer.Deserialize(json, type, option) as SettingsModelBase;
    }

    public static ShopSettingsModel? GetShopSettingsFromJson(this string json, ShopSettingType shopSettingType)
    {
        return shopSettingType == ShopSettingType.Product
            ? json.DeserializeWithNumberHandling<ProductShopSettingsModel>()
            : json.DeserializeWithNumberHandling<CategoryShopSettingsModel>();
    }
}
