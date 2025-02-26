using Alchemist.Product.Interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alchemist.Product.Import.Model.Infrastructure;

public static class ModelExtensions
{
    public static SettingsModelBase GetSettings(this ShopImportModel shopImport, TabType tab, bool last = false)
    {
        switch (tab)
        {
            case TabType.Shop:
                {
                    shopImport.ShopSettingTabs ??= new ShopSettingTabsModel() { ShopGuid = shopImport.ShopGuid };
                    if(!last) return shopImport.ShopSettingTabs;

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

            case TabType.Products:
                {
                    shopImport.ImportProducts ??= new ProductsImportSettingsModel() { ShopGuid = shopImport.ShopGuid };
                    return shopImport.ImportProducts;
                }

            case TabType.Categories:
                {
                    shopImport.ImportCategories ??= new CategoriesImportSettingsModel() { ShopGuid = shopImport.ShopGuid };
                    return shopImport.ImportCategories;
                }

            default:
                {
                    shopImport.ShopSettingTabs ??= new ShopSettingTabsModel() { ShopGuid = shopImport.ShopGuid };
                    return shopImport.ShopSettingTabs;
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
                    shopSettingTabs.ShopProductsSettings ??= new ProductShopSettingsModel()
                    {
                        ShopGuid = shopSettingTabs.ShopGuid
                    };
                    return shopSettingTabs.ShopProductsSettings;
                }

            case ShopSettingType.Category:
                {
                    shopSettingTabs.ShopCategoriesSettings ??= new CategoryShopSettingsModel()
                    {
                        ShopGuid = shopSettingTabs.ShopGuid
                    };
                    return shopSettingTabs.ShopCategoriesSettings;
                }

            default:
                return null;
        }
    }


    [Obsolete("Update with serviceName parameter")]
    public static bool UpdateServiceSettings(this ShopSettingsModel shopSettings, ServiceSettingsModel? serviceSettings)
    {
        if (serviceSettings == null) return false;
        switch (serviceSettings?.ServiceName)
        {
            case nameof(ShopSettingsModel.ImportService):
                {
                    shopSettings.ImportService ??= new ServiceSettingsModel { ShopGuid = shopSettings.ShopGuid, ShopSettingsId = shopSettings.Id };
                    shopSettings.ImportService.Update(serviceSettings);

                    if (!shopSettings.Services.Any(s => s.Guid == serviceSettings?.Guid))
                        shopSettings.Services.Add(shopSettings.ImportService);

                    return true;
                }

            case nameof(ShopSettingsModel.WebLoader):
                {
                    shopSettings.WebLoader ??= new ServiceSettingsModel { ShopGuid = shopSettings.ShopGuid, ShopSettingsId = shopSettings.Id };
                    shopSettings.WebLoader.Update(serviceSettings);

                    if (!shopSettings.Services.Any(s => s.Guid == serviceSettings?.Guid))
                        shopSettings.Services.Add(shopSettings.WebLoader);
                    return true;
                }

            case nameof(ShopSettingsModel.BrowserDataLoader):
                {
                    shopSettings.BrowserDataLoader ??= new ServiceSettingsModel { ShopGuid = shopSettings.ShopGuid, ShopSettingsId = shopSettings.Id };
                    shopSettings.BrowserDataLoader.Update(serviceSettings);

                    if (!shopSettings.Services.Any(s => s.Guid == serviceSettings?.Guid))
                        shopSettings.Services.Add(shopSettings.BrowserDataLoader);

                    return true;
                }

            default:
                return false;
        }
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
