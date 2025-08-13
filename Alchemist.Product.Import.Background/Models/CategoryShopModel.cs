using Alchemist.Import.Category.Interfaces;


namespace Alchemist.Product.Import.Background.Models;

internal class CategoryShopModel : ShopModel, ICategoryShopModel
{
    public string CategorySourceUrl { get; set; }
}
