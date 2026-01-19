using Alchemist.Import.Products.Interfaces;
using Import.Settings.Interfaces;
using Shop.Interfaces;
using System.ComponentModel;

namespace Alchemist.Product.Import.Background.Models;

interface IShopModel : IImportSource, IShop 
{
    IList<IProductShopCategory> RootCategories { get; }
}

internal class ShopModel : IShopModel
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

    public List<IProductShopCategory> RootCategories { get; } = [];

    string IShop.Name { get => Name; set => Name = value; }
    string IShop.Url { get => Url; set => Url = value; }

    IList<IProductShopCategory> IShopModel.RootCategories => RootCategories;

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string prop = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
