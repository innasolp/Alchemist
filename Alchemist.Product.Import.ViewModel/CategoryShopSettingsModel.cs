using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Interfaces;

namespace Alchemist.Product.Import.Model;

public class CategoryShopSettingsModel : ShopSettingsModel, ICategoryShopImportSettings
{
    public override ShopSettingType ShopSettingType => ShopSettingType.Category;

    public override string ToString()
    {
        return @$"{nameof(CategoryShopSettingsModel)}:{base.ToString()}";
    }
}
