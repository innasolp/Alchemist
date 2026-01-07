using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Settings.Product;
using System.Collections.ObjectModel;

namespace Alchemist.Product.Import.Background.Models;

interface IProductShopModel : IShopModel, IProductShopSource { }

internal class ProductShopModel : ShopModel, IProductShopModel
{
    private string _productUrl;
    public string ProductUrl
    {
        get => _productUrl;
        set
        {
            if (_productUrl != value)
            {
                _productUrl = value;
                OnPropertyChanged("ProductUrl");
            }
        }
    }

    private string _categoryUrl;
    public string CategoryUrl
    {
        get => _categoryUrl;
        set
        {
            if (_categoryUrl != value)
            {
                _categoryUrl = value;
                OnPropertyChanged("CategoryUrl");
            }
        }
    }

    public ObservableCollection<IProductShopCategory> Categories { get; } = [];

    public int? PageProductCount { get; set; }
    
    IEnumerable<IProductShopCategory> IProductShopSource.Categories => Categories;
}
