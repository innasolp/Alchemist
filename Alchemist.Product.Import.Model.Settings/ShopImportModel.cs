namespace Alchemist.Product.Import.Model;

public class ShopImportModel(IShopModel shop)
{
    public Guid ShopGuid { get; private set; } = shop.Guid;

    public IShopModel Shop { get; private set; } = shop;

    public IShopSettingTabsModel ShopSettingTabs { get; internal set; }

    public ICategoryImportSettingsModel ImportCategories { get; internal set; }

    public IProductImportSettingsModel ImportProducts { get; internal set; }
}
