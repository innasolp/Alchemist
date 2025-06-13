using Alchemist.Import.Settings.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Alchemist.Product.Import.Model;

public class CategoryUrlModel : ICategoryUrl
{
    [Required]
    [Range(0, int.MaxValue)]
    public int Item { get; set; }

    [Required]
    public string Url { get; set; }

    public Guid Guid { get; set; } = Guid.NewGuid();

    public Guid ShopSettingsGuid { get; set; }

    public void Update(CategoryUrlModel model)
    {
        Item = model.Item;
        Url = model.Url;
    }
}

public class ProductShopSettingsModel : ShopSettingsModel, IProductShopImportSettings
{
    [Required]
    public string? ProductUrlFormat { get; set; }

    [Required]
    public string? CategoryUrlFormat { get; set; }

    public int? PageProductCount { get; set; }

    public override ShopSettingType ShopSettingType => ShopSettingType.Product;

    public List<CategoryUrlModel> RootCategories { get; set; } = [];

    ICategoryUrl[]? IProductShopImportSettings.RootCategories
    { 
        get => RootCategories?.ToArray(); 
        set => RootCategories = value != null ? [.. value.OfType<CategoryUrlModel>()] : null;
    }

    public override string ToString()
    {
        return @$"{nameof(ProductShopSettingsModel)}:{base.ToString()};
                {nameof(ProductUrlFormat)}:{ProductUrlFormat};{nameof(CategoryUrlFormat)}:{CategoryUrlFormat};{nameof(PageProductCount)}:{PageProductCount}";
    }

    public override void Update(ShopSettingsModel sourceShopSettings, bool setNullServices = false)
    {
        if(sourceShopSettings is ProductShopSettingsModel productShopSettingsModel)
        {
            ProductUrlFormat = productShopSettingsModel.ProductUrlFormat;
            CategoryUrlFormat = productShopSettingsModel.CategoryUrlFormat;
        }
        base.Update(sourceShopSettings, setNullServices);
    }
}
