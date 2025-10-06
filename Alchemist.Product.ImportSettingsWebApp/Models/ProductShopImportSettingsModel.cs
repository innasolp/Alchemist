using Alchemist.Import.Settings.Interfaces;
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

    ICategoryUrl[]? IProductShopImportSettings.RootCategories
    {
        get => [.. RootCategories];
        set
        {
            RootCategories.Clear();
            RootCategories.AddRange(value?.OfType<CategoryUrlModel>() ?? []);
        }
    }
}
