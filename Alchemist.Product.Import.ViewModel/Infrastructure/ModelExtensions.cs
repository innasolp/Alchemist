using Alchemist.Product.Interfaces;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Alchemist.Product.Import.Model.Infrastructure;

public static class ModelExtensions
{
    public static SettingsModelBase GetSettings(this ShopImportModel shopImport, TabType tab, bool last = false)
    {
        switch (tab)
        {
            case TabType.Shop:
                {
                    return shopImport.GetShopSettingsModel(last);
                }

            case TabType.Products:
                {
                    shopImport.ImportProducts ??= new ProductsImportSettingsModel() { ShopGuid = shopImport.ShopGuid, ShopId = shopImport.Shop.Id };
                    return shopImport.ImportProducts;
                }

            case TabType.Categories:
                {
                    shopImport.ImportCategories ??= new CategoriesImportSettingsModel() { ShopGuid = shopImport.ShopGuid, ShopId = shopImport.Shop.Id };
                    return shopImport.ImportCategories;
                }

            default:
                {
                    return shopImport.GetShopSettingsModel(last);
                }
        }
    }

    private static SettingsModelBase GetShopSettingsModel(this ShopImportModel shopImport, bool last = false )
    {
        shopImport.ShopSettingTabs ??= new ShopSettingTabsModel() { ShopGuid = shopImport.ShopGuid, ShopId = shopImport.Shop.Id };
        if (!last) return shopImport.ShopSettingTabs;

        if (shopImport.ShopSettingTabs.SelectedSettingsTab == ShopSettingType.Product)
        {
            shopImport.ShopSettingTabs.ShopProductsSettings ??= new ProductShopSettingsModel();
            return shopImport.ShopSettingTabs.ShopProductsSettings;
        }
        else
        {
            shopImport.ShopSettingTabs.ShopCategoriesSettings ??= new CategoryShopSettingsModel();
            return shopImport.ShopSettingTabs.ShopCategoriesSettings;
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
                    shopSettingTabs.ShopProductsSettings ??= new ProductShopSettingsModel()
                    {
                        ShopGuid = shopSettingTabs.ShopGuid,
                        ShopId = shopSettingTabs.ShopId
                    };
                    return shopSettingTabs.ShopProductsSettings;
                }

            case ShopSettingType.Category:
                {
                    shopSettingTabs.ShopCategoriesSettings ??= new CategoryShopSettingsModel()
                    {
                        ShopGuid = shopSettingTabs.ShopGuid,
                        ShopId = shopSettingTabs.ShopId
                    };
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
        switch (serviceSettings?.ServiceName)
        {
            case nameof(ShopSettingsModel.ImportService):
                {
                    shopSettings.UpdateShopServiceSettings(serviceSettings, s => s.ImportService, (s, sm) => s.ImportService = sm);

                    return true;
                }

            case nameof(ShopSettingsModel.WebLoader):
                {
                    shopSettings.UpdateShopServiceSettings(serviceSettings, s => s.WebLoader, (s, sm) => s.WebLoader = sm);
                    return true;
                }

            case nameof(ShopSettingsModel.BrowserDataLoader):
                {
                    shopSettings.UpdateShopServiceSettings(serviceSettings, s => s.BrowserDataLoader, (s, sm) => s.BrowserDataLoader = sm);

                    return true;
                }

            default:
                return false;
        }
    }

    private static void UpdateShopServiceSettings(this ShopSettingsModel shopSettings, ServiceSettingsModel? serviceSettings,
        Func<ShopSettingsModel, ServiceSettingsModel> get,
        Action<ShopSettingsModel,ServiceSettingsModel> set)
    {
        if (get(shopSettings) == null)
            set(shopSettings, new ServiceSettingsModel
            {
                ShopGuid = shopSettings.ShopGuid,
                ShopSettingsGuid = shopSettings.Guid,
                ParentSettingsId = shopSettings.Id
            });
        get(shopSettings).Update(serviceSettings);

        if (!shopSettings.Services.Any(s => s.Guid == serviceSettings?.Guid))
            shopSettings.Services.Add(get(shopSettings));
    }

    public static void UpdateSettings(this ShopImportModel shopImport, SettingsModelBase settings)
    {
        switch (settings.Tab)
        {
            case TabType.Shop:
                shopImport.ShopSettingTabs.Update(settings);
                break;

            case TabType.Products:
                shopImport.ImportProducts.Update(settings);
                break;

            case TabType.Categories:
                shopImport.ImportCategories.Update(settings);
                break;

            default:
                shopImport.ShopSettingTabs.Update(settings);
                break;

        }
    }    

    public static void UpdateFromJson(this SettingsModelBase settingsModel, string json)
    {
        var option = new JsonSerializerOptions { NumberHandling = JsonNumberHandling.AllowReadingFromString, };
        var model = JsonSerializer.Deserialize(json, settingsModel.GetType(), option) as SettingsModelBase;
        settingsModel.Update(model);
    }

    public static T? DeserializeWithNumberHandling<T>(this string json)
        where T : class
    {
        var option = new JsonSerializerOptions { NumberHandling = JsonNumberHandling.AllowReadingFromString, };
        return JsonSerializer.Deserialize<T>(json, option);
    }
}
