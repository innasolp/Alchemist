using Alchemist.Import.Settings.Interfaces;
using System.ComponentModel.DataAnnotations;


namespace Alchemist.Product.Import.Model;

public class CategoryShopSettingsModel : ShopSettingsModel, ICategoryShopImportSettings
{
    public override ShopSettingType ShopSettingType => ShopSettingType.Category;

    [Required]
    public string CategorySourceUrl { get; set; }

    public override string ToString()
    {
        return @$"{nameof(CategoryShopSettingsModel)}:{base.ToString()}";
    }

    public override void Update(ShopSettingsModel sourceShopSettings, bool setNullServices = false)
    {
        if (sourceShopSettings is CategoryShopSettingsModel categoryShopSettingsModel)
        {
            CategorySourceUrl = categoryShopSettingsModel.CategorySourceUrl;
        }
        base.Update(sourceShopSettings, setNullServices);
    }
}
