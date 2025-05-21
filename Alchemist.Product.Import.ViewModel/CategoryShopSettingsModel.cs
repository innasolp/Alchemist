using Alchemist.Import.Settings.Interfaces;


namespace Alchemist.Product.Import.Model;

public class CategoryShopSettingsModel : ShopSettingsModel, ICategoryShopImportSettings
{
    public override ShopSettingType ShopSettingType => ShopSettingType.Category;

    public string CategorySourceUrl { get; set; }

    public override string ToString()
    {
        return @$"{nameof(CategoryShopSettingsModel)}:{base.ToString()}";
    }
}
