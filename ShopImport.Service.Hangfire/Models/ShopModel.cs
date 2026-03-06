using Alchemist.Import.Products.Interfaces;
using Import.Settings.Interfaces;
using Shop.Interfaces;
using System.ComponentModel;

namespace ShopImport.Service.Hangfire.Models;

interface IShopModel : IImportSource, IShop 
{
    IList<IProductShopCategory> RootCategories { get; }

    IEnumerable<IShopModel> Split();
}

internal abstract class ShopModel : IShopModel
{    
    public int Id { get; set; }

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

    protected abstract IEnumerable<IShopModel> Split();

    IEnumerable<IShopModel> IShopModel.Split()
    {
        return Split();
    }
}