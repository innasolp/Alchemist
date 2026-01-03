using Alchemist.Import.Settings.Product;
using Alchemist.Product.Interfaces;

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
        
    public override ShopSettingType Type => ShopSettingType.Product ;

    IEnumerable<ICategoryUrl>? IProductShopImportSettings.RootCategories { get => RootCategories; 
        set => RootCategories = value is TestCategoryUrl[] testCategoryUrls 
            ? testCategoryUrls
            : value != null ? value.OfType<TestCategoryUrl>().ToArray() : []; }
    public string? ProductHttpMethod { get; set; }
    public string? CategoryHttpMethod { get; set; }
    public UrlFormatType ProductUrlFormatType { get ; set; }
    public UrlFormatType CategoryUrlFormatType { get; set; }
}