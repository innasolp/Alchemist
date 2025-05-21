using Alchemist.Import.Category.Interfaces;


namespace Alchemist.Product.Import.Background;

internal class CategoryShopModel : ShopModel, ICategoryShopModel
{
    public string CategorySourceUrl { get; set; }
}
