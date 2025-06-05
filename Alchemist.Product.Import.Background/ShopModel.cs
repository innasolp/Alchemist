using Alchemist.Import.Interfaces;
using Alchemist.Product.Interfaces;
using System.ComponentModel;

namespace Alchemist.Product.Import.Background;

internal abstract class ShopModel : IShopModel, IShop
{
    private int _Id;
    public int Id
    {
        get => _Id;
        set
        {
            if (_Id != value)
            {
                _Id = value;
                OnPropertyChanged("Id");
            }
        }
    }

    public string ShopName { get; set; }
    public string ShopUrl { get; set; }
    public string? Caption { get; set; }
    string IShop.Name { get => ShopName; set => ShopName = value; }
    string IShop.Url { get => ShopUrl; set => ShopUrl = value; }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string prop = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
