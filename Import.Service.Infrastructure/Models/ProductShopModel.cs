using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Settings.Product;
using System.Collections.ObjectModel;

namespace Import.Service.Commands.Models;

interface IProductShopModel : IShopModel, IProductShopSource { }

internal class ProductShopModel : ShopModel, IProductShopModel
{
    
    public string ProductUrl{ get; set; }
    
    public string CategoryUrl{ get; set; }

    public ObservableCollection<IProductShopCategory> Categories { get; } = [];

    public int? PageProductCount { get; set; }
    
    IEnumerable<IProductShopCategory> IProductShopSource.Categories => Categories;
}