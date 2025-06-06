using Alchemist.Import.Settings.Interfaces;

namespace Alchemist.Import.Settings.Model;

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

    public CategoryUrl[]? RootCategories { get; set; }

    protected override ShopSettingType ShopSettingType => ShopSettingType.Product;

    ICategoryUrl[]? IProductShopImportSettings.RootCategories { get => RootCategories; set => RootCategories = (CategoryUrl[])value; }
}
