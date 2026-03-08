using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Settings.Product;
using System.Collections.ObjectModel;

namespace ShopImport.Service.Hangfire.Models;

interface IProductShopModel : IShopModel, IProductShopSource { }

internal class ProductShopModel : ShopModel, IProductShopModel
{    
    public string ProductUrl{ get; set; }
    
    public string CategoryUrl{ get; set; }

    public ObservableCollection<IProductShopCategory> Categories { get; } = [];

    public int? PageProductCount { get; set; }
    
    IEnumerable<IProductShopCategory> IProductShopSource.Categories => Categories;

    protected ProductShopModel CopyCore()
    {
        return new ProductShopModel
        {
            Name = Name,
            Caption = Caption,
            CategoryUrl = CategoryUrl,
            PageProductCount = PageProductCount,
            Id = Id,
            ProductUrl = ProductUrl,
            Url = Url
        };
    }

    protected override IEnumerable<IShopModel> Split()
    {
        foreach (var shopCategory in Categories)
        {
            var copy = CopyCore();
            copy.Name += $"_{shopCategory.Category}";
            copy.Categories.Add(shopCategory);
            yield return copy;
        }
    }
}