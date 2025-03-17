namespace Alchemist.Product.Import.Model;

public class ShopImportModel(ShopModel shop)
{
    public Guid ShopGuid { get; private set; } = shop.Guid;

    public ShopModel Shop { get; private set; } = shop;

    public ShopSettingTabsModel? ShopSettingTabs { get; set; }

    public CategoriesImportSettingsModel? ImportCategories { get; set; }

    public ProductsImportSettingsModel? ImportProducts { get; set; }
}
