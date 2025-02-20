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

    public static SettingsModelBase GetSettings(ShopImportModel shopImport, SettingsType type)
    {
        switch ( type)
        {
            case SettingsType.Shop:
            {
                    shopImport.ShopSettings ??= new ShopSettingsModel() { ShopId = shopImport.ShopId };
                    return shopImport.ShopSettings;
                }

            case SettingsType.Products:
                {
                    shopImport.ImportProducts ??= new ProductsImportSettingsModel() { ShopId = shopImport.ShopId };
                    return shopImport.ImportProducts;
                }

            case SettingsType.Categories:
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
}
