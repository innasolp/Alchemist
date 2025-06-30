namespace Alchemist.Product.Import.Model.Infrastructure;

public static class TabModelExtensions
{
    public static void UpdateProductImportSettings(this IProductImportSettingsModel target, IProductImportSettingsModel source  )
    {
        target.Name = source.Name;
    }

    public static void UpdateCategoryImportSettings(this ICategoryImportSettingsModel target, ICategoryImportSettingsModel source  )
    {
        target.Name = source.Name;
    }

    public static void UpdateShopSettings(this IShopSettingTabsModel target, IShopSettingTabsModel source)
    {
        target.ShopProductsSettings.UpdateProductShopSettings(source.ShopProductsSettings);
        target.ShopCategoriesSettings.UpdateCategoryShopSettings(source.ShopCategoriesSettings);
    }
}
