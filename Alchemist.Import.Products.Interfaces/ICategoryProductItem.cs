namespace Alchemist.Import.Products.Interfaces;

public interface ICategoryProductItem
{
    string Id { get; }

    string ItemUrl { get; }

    string Currency { get; }

    double Price { get; }

    string Name { get; }

    int CategoryItemId { get; set; }

    string? Brand { get; set; }
}