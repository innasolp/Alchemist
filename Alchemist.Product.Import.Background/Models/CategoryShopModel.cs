using Alchemist.Import.Settings.Category;


namespace Alchemist.Product.Import.Background.Models;

interface ICategoryShopModel: IShopModel, ICategoryShopSource { }

internal class CategoryShopModel : ShopModel, ICategoryShopModel
{
    public string CategorySourceUrl { get; set; }
}
