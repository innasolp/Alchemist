using Alchemist.Import.Products.Interfaces;
using Json.CustomSerialization;
using System.Collections.ObjectModel;

namespace Alchemist.Product.BeautyAndHealth.ImportItemHandler;

internal class BeautyAndHealthProduct : JsonItem, IProductItem
{
    private static readonly ReadOnlyDictionary<string, Type> _propertyTypes = new(new Dictionary<string, Type>()
    {
          { nameof(Name), typeof(string) },
          { nameof(Description), typeof(string) },
          { nameof(ItemId), typeof(string) },
          { nameof(Components), typeof(string[]) },
          { nameof(Country), typeof(string) },
          { nameof(Comment), typeof(string) },
          { nameof(ProductType), typeof(string) },
          { nameof(Brand), typeof(string) },
          { nameof(Purposes), typeof(string[]) },
          { nameof(Articul), typeof(string) }
    });

    public string Name
    {
        get => GetPropertyValue<string>(nameof(Name));
        set => SetPropertyValue(nameof(Name), value);
    }

    public string Description
    {
        get => GetPropertyValue<string>(nameof(Description));
        set => SetPropertyValue(nameof(Description), value);
    }

    public string ItemId
    {
        get => GetPropertyValue<string>(nameof(ItemId));
        set => SetPropertyValue(nameof(ItemId), value);
    }

    public string[]? Components
    {
        get => GetPropertyValue<string[]>(nameof(Components));
        set => SetPropertyValue(nameof(Components), value);
    }

    public string? Brand
    {
        get => GetPropertyValue<string>(nameof(Brand));
        set => SetPropertyValue(nameof(Brand), value);
    }

    public string? Country
    {
        get => GetPropertyValue<string>(nameof(Country));
        set => SetPropertyValue(nameof(Country), value);
    }

    public string Comment
    {
        get => GetPropertyValue<string>(nameof(Comment));
        set => SetPropertyValue(nameof(Comment), value);
    }

    public string ProductType
    {
        get => GetPropertyValue<string>(nameof(ProductType));
        set => SetPropertyValue(nameof(ProductType), value);
    }

    public string[] Purposes
    {
        get => GetPropertyValue<string[]>(nameof(Purposes));
        set => SetPropertyValue(nameof(Purposes), value);
    }

    public string Articul
    {
        get => GetPropertyValue<string>(nameof(Articul));
        set => SetPropertyValue(nameof(Articul), value);
    }

    public string Currency { get; set; }

    public double Price { get; set; }

    public string Path { get; set; }

    public string ApiUrl { get; set; }

    public int CategoryId { get; set; }

    public override IReadOnlyDictionary<string, Type> PropertyTypes => _propertyTypes;
}