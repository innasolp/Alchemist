using Alchemist.Import.Settings.Interfaces;

namespace Alchemist.Product.Import.Model;

public interface IShopSettingTabsModel : ITabModel
{ 
    IProductShopSettingsModel ShopProductsSettings { get; }

    ICategoryShopSettingsModel ShopCategoriesSettings { get; }
}
