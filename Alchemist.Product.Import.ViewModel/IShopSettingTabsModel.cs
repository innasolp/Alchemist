using Alchemist.Import.Settings.Interfaces;

namespace Alchemist.Product.Import.Model;

public interface IShopSettingTabsModel : ITabModel
{   
    ShopSettingType SelectedSettingsTab { get; set; }

    IProductShopSettingsModel ShopProductsSettings { get; }

    ICategoryShopSettingsModel ShopCategoriesSettings { get; }
}
