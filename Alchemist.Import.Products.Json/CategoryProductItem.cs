using Alchemist.Import.Products.Interfaces;
using Json.CustomSerialization;
using System.Collections.ObjectModel;

namespace Alchemist.Import.Products.Json;

public class CategoryProductItem : JsonItem, ICategoryProductItem
{
    private static readonly ReadOnlyDictionary<string, Type> _propertyTypes = new(new Dictionary<string, Type>()
    {
          { nameof(ICategoryProductItem.Name), typeof(string) },
          { nameof(ICategoryProductItem.Id), typeof(string) },
          { nameof(ICategoryProductItem.ItemPath), typeof(string) },
          { nameof(ICategoryProductItem.Currency), typeof(string) },
          { nameof(ICategoryProductItem.Price), typeof(double) },
          { nameof(ICategoryProductItem.Brand), typeof(string) }
    });

    public override IReadOnlyDictionary<string, Type> PropertyTypes => _propertyTypes;

    public string Name
    {
        get => GetPropertyValue<string>(nameof(Name));
        set => SetPropertyValue(nameof(Name), value);
    }

    public string Id
    {
        get => GetPropertyValue<string>(nameof(Id));
        set => SetPropertyValue(nameof(Id), value);
    }

    public string ItemPath
    {
        get => GetPropertyValue<string>(nameof(ItemPath));
        set => SetPropertyValue(nameof(ItemPath), value);
    }

    public string Currency
    {
        get => GetPropertyValue<string>(nameof(Currency));
        set => SetPropertyValue(nameof(Currency), value);
    }

    public double Price
    {
        get => GetPropertyValue<double>(nameof(Price));
        set => SetPropertyValue(nameof(Price), value);
    }

    public int CategoryItemId{ get; set; }

    public string? Brand
    {
        get => GetPropertyValue<string?>(nameof(Brand));
        set => SetPropertyValue(nameof(Brand), value);
    }
}