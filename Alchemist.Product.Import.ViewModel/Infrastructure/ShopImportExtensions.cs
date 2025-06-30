namespace Alchemist.Product.Import.Model.Infrastructure;

public static class ShopImportExtensions
{
    public static ITabModel GetTab(this ShopImportModel shopImport, TabType tab)
    {
        return tab == TabType.Shop
             ? shopImport.ShopSettingTabs
             : tab == TabType.Products ? shopImport.ImportProducts : shopImport.ImportCategories;
    }

    public static ISettingsModel GetLastSelectedSettings(this ShopImportModel shopImport, TabType tab)
    {
        return tab switch
        {
            TabType.Shop => shopImport.ShopSettingTabs.GetShopSettingsByType(shopImport.ShopSettingTabs.SelectedSettingsTab),
            TabType.Products => shopImport.ImportProducts,
            TabType.Categories => shopImport.ImportCategories,
            _ => throw new InvalidOperationException($"No tab type with value {tab}"),
        };
    }
}
