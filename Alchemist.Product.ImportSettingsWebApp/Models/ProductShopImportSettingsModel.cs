using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Settings.Product;
using Alchemist.Product.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Alchemist.Product.ImportSettingsWebApp.Models;

public class ProductShopImportSettingsModel : ShopImportSettingsModel, IProductShopImportSettings
{
    [JsonIgnore]
    public override ShopSettingType ShopSettingType => ShopSettingType.Product;

    [Required]
    public string? ProductUrlFormat { get; set; }

    [Required]
    public string? CategoryUrlFormat { get; set; }

    public int? PageProductCount { get; set; }

    public List<CategoryUrlModel> RootCategories { get; set; } = [];
    public UrlFormatType ProductUrlFormatType { get; set; }
    public UrlFormatType CategoryUrlFormatType { get; set; }
    IEnumerable<ICategoryUrl>? IProductShopImportSettings.RootCategories 
    { 
        get => RootCategories; 
        set => RootCategories = value is List<CategoryUrlModel> categoryUrls
            ? categoryUrls 
            : value != null ? [.. value.OfType<CategoryUrlModel>()] : [];
    }
}