using Alchemist.Product.Import.WebApp.Infrastructure;
using Alchemist.Product.Interfaces;
using System.Text.Json.Serialization;

namespace Alchemist.Product.Import.WebApp.Models;

public class ShopSettingTabsModel : SettingsModelBase
{
    [JsonIgnore]
    public override TabType Tab => TabType.Shop;

    public ShopSettingsModel? ShopProductsSettings { get; set; }

    public ShopSettingsModel? ShopCategoriesSettings { get; set; }
    
    public ShopSettingType SelectedSettingsTab { get; set; } = ShopSettingType.Product;

    public override void Update(SettingsModelBase source)
    {
        base.Update(source);

        if (source is not ShopSettingTabsModel shopSettingTabsModel)
            throw new InvalidCastException($"invalid source {source.GetType().Name} for {GetType().Name}");

        SelectedSettingsTab = shopSettingTabsModel.SelectedSettingsTab;

        if (shopSettingTabsModel.ShopProductsSettings != null)
        {
            ShopProductsSettings ??= new ShopSettingsModel { ShopId = ShopId, ShopSettingType = ShopSettingType.Product };
            ShopProductsSettings.Update(shopSettingTabsModel.ShopProductsSettings);
        }
        
        if (shopSettingTabsModel.ShopCategoriesSettings != null)
        {
            ShopCategoriesSettings ??= new ShopSettingsModel { ShopId = ShopId, ShopSettingType = ShopSettingType.Category };
            ShopCategoriesSettings.Update(shopSettingTabsModel.ShopCategoriesSettings);
        }
    }
}
