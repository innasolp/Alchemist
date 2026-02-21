using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Settings.Product;
using ShopImport.Service.Infrastructure.Module.Models;
using System.Collections.ObjectModel;

namespace ShopImport.Service.Hangfire.Models;

interface IProductShopModel : IShopModel, IProductShopSource { }

internal class ProductShopModel : ShopModel, IProductShopModel
{
    
    public string ProductUrl{ get; set; }
    
    public string CategoryUrl{ get; set; }

    public ObservableCollection<IProductShopCategory> Categories { get; } = [];

    public int? PageProductCount { get; set; }
    
    IEnumerable<IProductShopCategory> IProductShopSource.Categories => Categories;
}