using Alchemist.Import.Interfaces;
using Alchemist.Product.Interfaces;
using System.ComponentModel;

namespace Alchemist.Product.Import.Background;

internal class ShopModel : IShopModel, IShop
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

    public string Name { get; set; }
    public string Url { get; set; }
    public string? Caption { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string prop = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
