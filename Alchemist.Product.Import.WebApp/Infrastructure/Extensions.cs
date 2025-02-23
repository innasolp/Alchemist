using Alchemist.Product.Import.WebApp.Models;
using System.Text.Json;

namespace Alchemist.Product.Import.WebApp.Infrastructure;

public static class Extensions
{
    public static SettingsModelBase GetSettings(this ShopImportModel shopImport, string tabName)
    {
        switch (tabName)
        {
            case "Settings":
                {
                    shopImport.ShopSettings ??= new ShopSettingsModel() { ShopId = shopImport.ShopId };
                    return shopImport.ShopSettings;
                }

            case "ImportProducts":
                {
                    shopImport.ImportProducts ??= new ProductsImportSettingsModel() { ShopId = shopImport.ShopId };
                    return shopImport.ImportProducts;
                }

            case "ImportCategories":
                {
                    shopImport.ImportCategories ??= new CategoriesImportSettingsModel() { ShopId = shopImport.ShopId };
                    return shopImport.ImportCategories;
                }

            default:
                {
                    shopImport.ShopSettings ??= new ShopSettingsModel() { ShopId = shopImport.ShopId };
                    return shopImport.ShopSettings;
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
                shopImport.ShopSettings.Update(settings);
                break;

            case TabType.Products:
                shopImport.ImportProducts.Update(settings);
                break;

            case TabType.Categories:
                shopImport.ImportCategories.Update(settings);
                break;

            default:
                shopImport.ShopSettings.Update(settings);
                break;

        }
    }

    public static SettingsModelBase GetSettings(this ShopImportModel shopImport, TabType type)
    {
        switch (type)
        {
            case TabType.Shop:
                {
                    shopImport.ShopSettings ??= new ShopSettingsModel() { ShopId = shopImport.ShopId };
                    return shopImport.ShopSettings;
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
                    shopImport.ShopSettings ??= new ShopSettingsModel() { ShopId = shopImport.ShopId };
                    return shopImport.ShopSettings;
                }
        }
    }

    public static SettingsModelBase? GetSettingsByTypeFromJson(this TabType settingsType, string json)
    {
        Type settingsModelType;
        switch (settingsType)
        {
            case TabType.Shop:
                settingsModelType = typeof(ShopSettingsModel);
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

        var option = new JsonSerializerOptions { NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString };
        var settingsModel = JsonSerializer.Deserialize(json, settingsModelType, option) as SettingsModelBase;
        return settingsModel;
    }
}
