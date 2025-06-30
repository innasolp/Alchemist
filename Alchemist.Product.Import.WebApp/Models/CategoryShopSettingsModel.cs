using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Import.Model;
using System.ComponentModel.DataAnnotations;


namespace Alchemist.Product.Import.WebApp.Models;

public class CategoryShopSettingsModel : ShopSettingsModel, ICategoryShopSettingsModel
{
    public override ShopSettingType ShopSettingType => ShopSettingType.Category;

    [Required]
    public string CategorySourceUrl { get; set; }

    public CategoryShopSettingsModel(int shopId, int id, Guid shopGuid) : base(shopId, id, shopGuid)
    {
    }

    public CategoryShopSettingsModel():this(0,0,Guid.Empty)
    { }

    public override string ToString()
    {
        return @$"{nameof(CategoryShopSettingsModel)}:{base.ToString()}";
    }
}
