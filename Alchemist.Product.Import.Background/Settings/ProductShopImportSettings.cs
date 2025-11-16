using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Interfaces;

namespace Alchemist.Product.Import.Background.Settings;

public class CategoryUrl : ICategoryUrl
{
    public int Item { get; set; }
    public string Url { get; set; }
}

public class ProductShopImportSettings : ShopImportSettings, IProductShopImportSettings
{   
    public string? ProductUrlFormat { get; set; }

    public string? CategoryUrlFormat { get; set; }

    public int? PageProductCount { get; set; }

    public CategoryUrl[]? RootCategories { get; set; } = [];

    public override ShopSettingType ShopSettingType => ShopSettingType.Product;

    IEnumerable<ICategoryUrl>? IProductShopImportSettings.RootCategories { get => RootCategories; set => RootCategories = (CategoryUrl[])value; }
}
