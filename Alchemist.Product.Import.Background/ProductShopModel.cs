using Alchemist.Import.Products.Interfaces;
using Alchemist.Product.Interfaces;
using System.Collections.ObjectModel;

namespace Alchemist.Product.Import.Background;

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

    public ObservableCollection<IShopCategory> Categories { get; } = [];
    public int? PageProductCount { get; set; }    
}
