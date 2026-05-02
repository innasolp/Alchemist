using Alchemist.Import.Settings.Category;

namespace ShopImport.Service.Infrastructure.Module.Models;

interface ICategoryShopModel: IShopModel, ICategoryShopSource { }

internal class CategoryShopModel : ShopModel, ICategoryShopModel
{
    public string CategorySourceUrl { get; set; }
}
