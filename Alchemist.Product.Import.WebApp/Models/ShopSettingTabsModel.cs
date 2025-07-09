using Alchemist.Product.Import.Model.Infrastructure;
using System.Text.Json.Serialization;
using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Import.Model;

namespace Alchemist.Product.Import.WebApp.Models;

[method: JsonConstructor]
public class ShopSettingTabsModel(int shopId, Guid shopGuid) : SettingsModelBase(shopId, shopGuid), IShopSettingTabsModel
{
    [JsonIgnore]
    public override TabType Tab => TabType.Shop;

    public ProductShopSettingsModel? ShopProductsSettings { get; } = new ProductShopSettingsModel(shopId, 0, shopGuid);

    public CategoryShopSettingsModel? ShopCategoriesSettings { get; } = new CategoryShopSettingsModel(shopId,0, shopGuid);
    
    public ShopSettingType SelectedSettingsTab { get; set; } = ShopSettingType.Product;
    
    IProductShopSettingsModel IShopSettingTabsModel.ShopProductsSettings => ShopProductsSettings;

    ICategoryShopSettingsModel IShopSettingTabsModel.ShopCategoriesSettings => ShopCategoriesSettings;
}
