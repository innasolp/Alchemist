using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Settings.Category;

namespace ShopImport.Service.Hangfire.Models;

interface ICategoryShopModel: IShopModel, ICategoryShopSource { }

internal class CategoryShopModel : ShopModel, ICategoryShopModel
{
    public string CategorySourceUrl { get; set; }

    protected override ShopModelType Type => ShopModelType.Category;

    protected CategoryShopModel CopyCore()
    {
        return new CategoryShopModel
        {
            Id = Id,
            Caption = Caption,
            Name = Name,
            CategorySourceUrl = CategorySourceUrl,
            Url = Url,
        };
    }
    protected override IEnumerable<IShopModel> Split()
    {
        foreach (var rootCategory in RootCategories)        
            yield return CreateByCategory(rootCategory);
    }
    
    protected override IShopModel AddSourceItem(IProductShopCategory shopCategory)
    {
        RootCategories.Add(shopCategory);

        return CreateByCategory(shopCategory);
    }

    private CategoryShopModel CreateByCategory(IProductShopCategory shopCategory)
    {
        var copy = CopyCore();
        copy.Name += $"_{shopCategory.Category}";
        copy.RootCategories.Add(shopCategory);
        return copy;
    }
}
