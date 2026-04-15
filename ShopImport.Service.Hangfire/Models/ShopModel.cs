using Alchemist.Import.Products.Interfaces;
using Import.Settings.Interfaces;
using Shop.Interfaces;
using ShopImport.Service.Hangfire.Infrastructure;
using System.ComponentModel;

namespace ShopImport.Service.Hangfire.Models;

enum ShopModelType
{
    Product = 1,
    Category = 2
}

interface IShopModel : 
    IImportSource,
    ISplittableSource,
    ISplittableSource<IShopModel>,
    IIdentificableSource ,
    ISourceItemCollection<IShopModel, IProductShopCategory>
{
    IList<IProductShopCategory> RootCategories { get; }

    ShopModelType Type { get; }
}

internal abstract class ShopModel : IShopModel, IShop
{
    public override string ToString()
    {
        return Name;
    }

    public int Id { get; set; }

    public string Name { get; set; }
    public string Url { get; set; }
    public string? Caption { get; set; }

    public List<IProductShopCategory> RootCategories { get; } = [];    

    IList<IProductShopCategory> IShopModel.RootCategories => RootCategories;

    protected abstract ShopModelType Type { get; }

    ShopModelType IShopModel.Type => Type;

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string prop = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    protected abstract IEnumerable<IShopModel> Split();

    protected abstract IShopModel AddSourceItem(IProductShopCategory item);

    IShopModel ISourceItemCollection<IShopModel, IProductShopCategory>.AddSourceItem(IProductShopCategory item)
    {
        return AddSourceItem(item); 
    }

    IEnumerable<IImportSource> ISplittableSource.Split()
    {
        return Split();
    }

    IEnumerable<IShopModel> ISplittableSource<IShopModel>.Split()
    {
        return Split();
    }
}