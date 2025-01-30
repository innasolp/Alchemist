using Alchemist.Product.Interfaces;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Alchemist.Product.Entities;

//todo move to other project
public class ShopUrlModel : IShopUrlModel
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

    private int _shopId;
    public int ShopId
    {
        get => _shopId;
        set
        {
            if (_shopId != value)
            {
                _shopId = value;
                OnPropertyChanged("ShopId");
            }
        }
    }

    public string Name { get; set; }
    public string Url { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;
    public void OnPropertyChanged(string prop = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
