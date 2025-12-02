using Alchemist.Product.Interfaces;
using Import.Settings.Interfaces;
using System.ComponentModel;

namespace Alchemist.Product.Import.Background.Models;

interface IShopModel : IImportSource, IShop { }

internal abstract class ShopModel : IShopModel
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
    string IShop.Name { get => Name; set => Name = value; }
    string IShop.Url { get => Url; set => Url = value; }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string prop = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
