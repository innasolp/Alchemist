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

    public static void UpdateShopSettings(this IModelFactory modelFactory, IShopSettingTabsModel target, IShopSettingTabsModel source)
    {
        modelFactory.UpdateProductShopSettings(target.ShopProductsSettings, source.ShopProductsSettings);
        modelFactory.UpdateCategoryShopSettings(target.ShopCategoriesSettings, source.ShopCategoriesSettings);
    }
}
