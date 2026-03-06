using Alchemist.Import.Settings.Category;

namespace ShopImport.Service.Hangfire.Models;

interface ICategoryShopModel: IShopModel, ICategoryShopSource { }

internal class CategoryShopModel : ShopModel, ICategoryShopModel
{
    public string CategorySourceUrl { get; set; }

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
        {
            var copy = CopyCore();
            copy.Name += $"_{rootCategory.Category}";
            copy.RootCategories.Add(rootCategory);
            yield return copy;
        }
    }
}
