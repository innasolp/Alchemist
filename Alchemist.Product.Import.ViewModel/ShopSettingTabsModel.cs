using Alchemist.Product.Import.Model.Infrastructure;
using System.Text.Json.Serialization;
using Alchemist.Import.Settings.Interfaces;

namespace Alchemist.Product.Import.Model;

public class ShopSettingTabsModel : SettingsModelBase
{
    [JsonIgnore]
    public override TabType Tab => TabType.Shop;

    public ProductShopSettingsModel? ShopProductsSettings { get; set; }

    public CategoryShopSettingsModel? ShopCategoriesSettings { get; set; }
    
    public ShopSettingType SelectedSettingsTab { get; set; } = ShopSettingType.Product;

    public override void Update(SettingsModelBase source)
    {
        base.Update(source);        

        if (source is not ShopSettingTabsModel shopSettingTabsModel)
            throw new InvalidCastException($"invalid source {source.GetType().Name} for {GetType().Name}");

        SelectedSettingsTab = shopSettingTabsModel.SelectedSettingsTab;

        if (shopSettingTabsModel.ShopProductsSettings != null)
        {
            ShopProductsSettings ??= new ProductShopSettingsModel { ShopGuid = ShopGuid };
            ShopProductsSettings.Update(shopSettingTabsModel.ShopProductsSettings);
        }
        
        if (shopSettingTabsModel.ShopCategoriesSettings != null)
        {
            ShopCategoriesSettings ??= new CategoryShopSettingsModel { ShopGuid = ShopGuid };
            ShopCategoriesSettings.Update(shopSettingTabsModel.ShopCategoriesSettings);
        }
    }
}
