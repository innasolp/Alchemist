using Alchemist.Import.Shop.Interfaces;
using System.ComponentModel;

namespace Alchemist.Product.Import.Background;

internal class ShopModel : IShopModel
{
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

    public string ShopName { get; set; }
    public string ShopUrl { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string prop = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
