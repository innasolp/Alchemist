using Alchemist.Import.Settings.Interfaces;

namespace Alchemist.Product.Import.Model;

public class CategoryUrlModel : ICategoryUrl
{
    public int Item { get; set; }
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
    public string? ProductUrlFormat { get; set; }
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
}
