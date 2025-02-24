using Alchemist.Product.Import.WebApp.Models;
using Alchemist.Product.Interfaces;
using System.Text.Json;

namespace Alchemist.Product.Import.WebApp.Infrastructure;

public static class Extensions
{
    public static SettingsModelBase GetSettings(this ShopImportModel shopImport, TabType tab)
    {
        switch (tab)
        {
            case TabType.Shop:
                {
                    shopImport.ShopSettingTabs ??= new ShopSettingTabsModel() { ShopId = shopImport.ShopId };
                    return shopImport.ShopSettingTabs;
                }

            case TabType.Products:
                {
                    shopImport.ImportProducts ??= new ProductsImportSettingsModel() { ShopId = shopImport.ShopId };
                    return shopImport.ImportProducts;
                }

            case TabType.Categories:
                {
                    shopImport.ImportCategories ??= new CategoriesImportSettingsModel() { ShopId = shopImport.ShopId };
                    return shopImport.ImportCategories;
                }

            default:
                {
                    shopImport.ShopSettingTabs ??= new ShopSettingTabsModel() { ShopId = shopImport.ShopId };
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
                    shopSettingTabs.ShopProductsSettings ??= new ShopSettingsModel()
                    {
                        ShopId = shopSettingTabs.ShopId,
                        ShopSettingType = ShopSettingType.Product
                    };
                    return shopSettingTabs.ShopProductsSettings;
                }

            case ShopSettingType.Category:
                {
                    shopSettingTabs.ShopCategoriesSettings ??= new ShopSettingsModel()
                    {
                        ShopId = shopSettingTabs.ShopId,
                        ShopSettingType = ShopSettingType.Category
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
                    shopSettings.ImportService ??= new ServiceSettingsModel { ShopId = shopSettings.ShopId, ShopSettingsId = shopSettings.Id };
                    shopSettings.ImportService.Update(serviceSettings);

                    if (!shopSettings.ServiceSettings.Any(s => s.Guid == serviceSettings?.Guid))
                        shopSettings.ServiceSettings.Add(shopSettings.ImportService);

                    return true;
                }

            case nameof(ShopSettingsModel.WebLoader):
                {
                    shopSettings.WebLoader ??= new ServiceSettingsModel { ShopId = shopSettings.ShopId, ShopSettingsId = shopSettings.Id };
                    shopSettings.WebLoader.Update(serviceSettings);

                    if (!shopSettings.ServiceSettings.Any(s => s.Guid == serviceSettings?.Guid))
                        shopSettings.ServiceSettings.Add(shopSettings.WebLoader);
                    return true;
                }

            case nameof(ShopSettingsModel.BrowserDataLoader):
                {
                    shopSettings.BrowserDataLoader ??= new ServiceSettingsModel { ShopId = shopSettings.ShopId, ShopSettingsId = shopSettings.Id };
                    shopSettings.BrowserDataLoader.Update(serviceSettings);

                    if (!shopSettings.ServiceSettings.Any(s => s.Guid == serviceSettings?.Guid))
                        shopSettings.ServiceSettings.Add(shopSettings.BrowserDataLoader);

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

    public static SettingsModelBase? GetSettingsByTypeFromJson(this TabType settingsType, string json)
    {
        Type settingsModelType;
        switch (settingsType)
        {
            case TabType.Shop:
                settingsModelType = typeof(ShopSettingTabsModel);
                break;

            case TabType.Products:
                settingsModelType = typeof(ProductsImportSettingsModel);
                break;

            case TabType.Categories:
                settingsModelType = typeof(CategoriesImportSettingsModel);
                break;

            default:
                settingsModelType = typeof(ShopSettingsModel);
                break;
        }

        var option = new JsonSerializerOptions { NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString, };
        var settingsModel = JsonSerializer.Deserialize(json, settingsModelType, option) as SettingsModelBase;
        return settingsModel;
    }

    public static T? DeserializeWithNumberHandling<T>(this string json)
        where T : class
    {
        var option = new JsonSerializerOptions { NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString, };
        return JsonSerializer.Deserialize<T>(json, option);
    }
}
