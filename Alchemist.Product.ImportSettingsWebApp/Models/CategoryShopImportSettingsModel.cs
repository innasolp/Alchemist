using Alchemist.Import.Settings.Category;
using Alchemist.Product.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Alchemist.Product.ImportSettingsWebApp.Models;

public class CategoryShopImportSettingsModel : ShopImportSettingsModel, ICategoryShopImportSettings
{
    [JsonIgnore]
    public override ShopSettingType ShopSettingType => ShopSettingType.Category;

    [Required]
    public string CategorySourceUrl { get; set; }
}