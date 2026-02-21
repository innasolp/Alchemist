using Alchemist.Import.Settings.Category;
using ShopImport.Service.Infrastructure.Module.Models;

namespace ShopImport.Service.Hangfire.Models;

interface ICategoryShopModel: IShopModel, ICategoryShopSource { }

internal class CategoryShopModel : ShopModel, ICategoryShopModel
{
    public string CategorySourceUrl { get; set; }
}
