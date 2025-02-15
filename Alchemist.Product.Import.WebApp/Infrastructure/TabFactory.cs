using Alchemist.Product.Import.WebApp.Models;
using System.Text.Json;

namespace Alchemist.Product.Import.WebApp.Infrastructure;

public static class TabFactory
{
    public static SettingsModelBase GetSettings(ShopImportModel shopImport, string tabName)
    {
        switch (tabName)
        {
            case "Settings":
                {
                    shopImport.ShopSettings ??= new ShopSettingsModel();
                    return shopImport.ShopSettings;
                }

            case "ImportProducts":
                {
                    shopImport.ImportProducts ??= new ProductsImportSettingsModel();
                    return shopImport.ImportProducts;
                }

            case "ImportCategories":
                {
                    shopImport.ImportCategories ??= new CategoriesImportSettingsModel();
                    return shopImport.ImportCategories;
                }

            default:
                {
                    shopImport.ShopSettings ??= new ShopSettingsModel();
                    return shopImport.ShopSettings;
                }
        }
    }

    public static SettingsModelBase? GetSettingsByTypeFromJson(SettingsType settingsType, string json)
    {
        Type settingsModelType;
        switch (settingsType)
        {
            case SettingsType.Shop:
                settingsModelType = typeof(ShopSettingsModel);
                break;

            case SettingsType.Products:
                settingsModelType = typeof(ProductsImportSettingsModel);
                break;

            case SettingsType.Categories:
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

    public static void UpdateSettings(ShopImportModel shopImport, SettingsModelBase settings)
    {
        switch (settings.Type)
        {
            case SettingsType.Shop:
                shopImport.ShopSettings.Update(settings);
                break;

            case SettingsType.Products:
                shopImport.ImportProducts.Update(settings);
                break;

            case SettingsType.Categories:
                shopImport.ImportCategories.Update(settings);
                break;

            default:
                shopImport.ShopSettings.Update(settings);
                break;

        }
    }
}
