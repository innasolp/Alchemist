namespace Alchemist.Product.Import.Model;

public class ShopImportModel
{
    public Guid ShopGuid { get; set; }

    public required ShopModel Shop { get; set; }

    public ShopSettingTabsModel? ShopSettingTabs { get; set; }

    public CategoriesImportSettingsModel? ImportCategories { get; set; }

    public ProductsImportSettingsModel? ImportProducts { get; set; }
}
