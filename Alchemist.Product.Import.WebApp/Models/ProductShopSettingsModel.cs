using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Import.Model;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Alchemist.Product.Import.WebApp.Models;

public class CategoryUrlModel(Guid shopSettingsGuid) : ICategoryUrlModel
{
    [Required]
    [Range(0, int.MaxValue)]
    public int Item { get; set; }

    [Required]
    public string Url { get; set; }

    public Guid Guid { get; set; } = Guid.NewGuid();

    public Guid ShopSettingsGuid { get; set; } = shopSettingsGuid;

    public CategoryUrlModel() : this(Guid.Empty) { }


    
}

public class ProductShopSettingsModel : ShopSettingsModel, IProductShopSettingsModel, IJsonOnDeserialized
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

    public ProductShopSettingsModel(int shopId, int id, Guid shopGuid) : base(shopId, id, shopGuid)
    {
    }

    public ProductShopSettingsModel() : base(0, 0, Guid.Empty) { }

    public override string ToString()
    {
        return @$"{nameof(ProductShopSettingsModel)}:{base.ToString()};
                {nameof(ProductUrlFormat)}:{ProductUrlFormat};{nameof(CategoryUrlFormat)}:{CategoryUrlFormat};{nameof(PageProductCount)}:{PageProductCount}";
    }

    public void OnDeserialized()
    {
        foreach (var rootCategory in RootCategories)
            rootCategory.ShopSettingsGuid = Guid;
    }
}
