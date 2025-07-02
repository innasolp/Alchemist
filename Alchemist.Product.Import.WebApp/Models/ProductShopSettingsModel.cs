using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Import.Model;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Alchemist.Product.Import.WebApp.Models;

[method: JsonConstructor]
public class CategoryUrlModel(Guid shopSettingsGuid) : ICategoryUrlModel
{
    [Required]
    [Range(0, int.MaxValue)]
    public int Item { get; set; }

    [Required]
    public string Url { get; set; }

    [JsonInclude]
    public Guid Guid { get; private set; } = Guid.NewGuid();

    [JsonInclude]
    public Guid ShopSettingsGuid { get; private set; } = shopSettingsGuid;
}

[method: JsonConstructor]
public class ProductShopSettingsModel(int shopId, int id, Guid shopGuid) 
    : ShopSettingsModel(shopId, id, shopGuid), IProductShopSettingsModel, IJsonOnDeserialized
{
    [Required]
    public string? ProductUrlFormat { get; set; }

    [Required]
    public string? CategoryUrlFormat { get; set; }

    public int? PageProductCount { get; set; }

    public override ShopSettingType ShopSettingType => ShopSettingType.Product;

    [JsonInclude]
    public List<CategoryUrlModel> RootCategories { get; private set; } = [];

    IList IProductShopSettingsModel.RootCategories => RootCategories;

    ICategoryUrl[]? IProductShopImportSettings.RootCategories { get => [.. RootCategories]; 
        set { 
            RootCategories.Clear();
            RootCategories.AddRange(value.OfType<CategoryUrlModel>()); 
        } }

    public override string ToString()
    {
        return @$"{nameof(ProductShopSettingsModel)}:{base.ToString()};
                {nameof(ProductUrlFormat)}:{ProductUrlFormat};{nameof(CategoryUrlFormat)}:{CategoryUrlFormat};{nameof(PageProductCount)}:{PageProductCount}";
    }

    public void OnDeserialized()
    {
        //todo
        //foreach (var rootCategory in RootCategories)
        //    rootCategory.ShopSettingsGuid = Guid;
    }
}
