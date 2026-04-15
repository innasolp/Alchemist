using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Settings.Product;
using System.Collections.Concurrent;

namespace ShopImport.Service.Hangfire.Models;

interface IProductShopModel : IShopModel, IProductShopSource { }

internal class ProductShopModel : ShopModel, IProductShopModel
{    
    public string ProductUrl{ get; set; }
    
    public string CategoryUrl{ get; set; }

    public BlockingCollection<IProductShopCategory> Categories { get; } = [];

    public int? PageProductCount { get; set; }
    
    IEnumerable<IProductShopCategory> IProductShopSource.Categories => Categories;

    protected override ShopModelType Type => ShopModelType.Product;

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
             yield return CreateByCategory(shopCategory);
        }
    }

    protected override IShopModel AddSourceItem(IProductShopCategory shopCategory)
    {
        Categories.TryAdd(shopCategory);

        return CreateByCategory(shopCategory);
    }

    private ProductShopModel CreateByCategory(IProductShopCategory shopCategory)
    {
        var copy = CopyCore();
        copy.Name += $"_{shopCategory.Category} {shopCategory.Path.Replace("/", "_")}";
        copy.Categories.TryAdd(shopCategory);
        return copy;
    }
}