using Alchemist.Import.Products.Interfaces;
using Json.CustomSerialization;
using System.Collections.ObjectModel;

namespace Alchemist.Import.Products.Json;

public class CategoryProducts : JsonItem, ICategoryProducts
{
    private static readonly ReadOnlyDictionary<string, Type> _propertyTypes = new(new Dictionary<string, Type>()
    {
          { nameof(ICategoryProducts.CategoryProductItems), typeof(CategoryProductItem[]) },
          { nameof(ICategoryProducts.TotalCount), typeof(int?) }
    });

    public override IReadOnlyDictionary<string, Type> PropertyTypes => _propertyTypes;

    public CategoryProductItem[] CategoryProductItems
    {
        get => GetPropertyValue<CategoryProductItem[]>(nameof(CategoryProductItems));
        set => SetPropertyValue(nameof(CategoryProductItems), value);
    }

    public int? TotalCount
    {
        get => GetPropertyValue<int?>(nameof(TotalCount));
        set => SetPropertyValue(nameof(TotalCount), value);
    }

    ICategoryProductItem[] ICategoryProducts.CategoryProductItems => CategoryProductItems;
}