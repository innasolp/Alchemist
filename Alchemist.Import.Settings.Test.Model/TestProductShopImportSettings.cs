using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Interfaces;
using System.Net.Http.Headers;

namespace Alchemist.Import.Settings.Test.Model;

public class TestProductShopImportSettings : TestShopImportSettings, IProductShopImportSettings
{
    public class TestCategoryUrl : ICategoryUrl
    {
        public int Item { get; set; }
        public string Url { get; set; }
    }

    public string? ProductUrlFormat { get; set; }

    public string? CategoryUrlFormat { get; set; }

    public int? PageProductCount { get; set; }

    public TestCategoryUrl[]? RootCategories { get; set; } = [];

    ICategoryUrl[]? IProductShopImportSettings.RootCategories { get => RootCategories; set => RootCategories = (TestCategoryUrl[])value; }

    public override ShopSettingType Type => ShopSettingType.Product ;
}
