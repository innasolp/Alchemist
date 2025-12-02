using Alchemist.Import.Settings.Category;


namespace Alchemist.Product.Import.Background.Models;

internal class CategoryShopModel : ShopModel, ICategoryShopModel
{
    public string CategorySourceUrl { get; set; }
}
