namespace Alchemist.Product.Interfaces;

public interface IShop
{
    int Id { get; set; }

    string Name { get; set; }

    string Url { get; set; }

    string? Caption { get; set; }
}
