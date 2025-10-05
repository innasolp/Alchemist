using Alchemist.Product.Import.Model;
using Alchemist.Product.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;


namespace Alchemist.Product.Import.WebApp.Models;

[method: JsonConstructor]
public partial class CategoryShopSettingsModel(int shopId, int id, Guid shopGuid) : 
    ShopSettingsModel(shopId, id, shopGuid), ICategoryShopSettingsModel
{
    public override ShopSettingType Type => ShopSettingType.Category;

    [Required]
    public string CategorySourceUrl { get; set; }

    public override string ToString()
    {
        return @$"{nameof(CategoryShopSettingsModel)}:{base.ToString()}";
    }
}
