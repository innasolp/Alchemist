using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Import.Model;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;


namespace Alchemist.Product.Import.WebApp.Models;

[method: JsonConstructor]
public class CategoryShopSettingsModel(int shopId, int id, Guid shopGuid) : 
    ShopSettingsModel(shopId, id, shopGuid), ICategoryShopSettingsModel
{
    public override ShopSettingType ShopSettingType => ShopSettingType.Category;

    [Required]
    public string CategorySourceUrl { get; set; }

    public override string ToString()
    {
        return @$"{nameof(CategoryShopSettingsModel)}:{base.ToString()}";
    }
}
