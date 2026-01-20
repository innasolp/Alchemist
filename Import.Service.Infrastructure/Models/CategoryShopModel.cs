using Alchemist.Import.Settings.Category;

namespace Import.Service.Commands.Models;

interface ICategoryShopModel: IShopModel, ICategoryShopSource { }

internal class CategoryShopModel : ShopModel, ICategoryShopModel
{
    public string CategorySourceUrl { get; set; }
}
