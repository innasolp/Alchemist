namespace Alchemist.Import.Products.Interfaces;

public interface IProductItem
{
    string Name { get; set; }

    string ItemId { get; set; }   

    string Brand { get; set; }

    string Currency { get; set; }

    double Price { get; set; }

    string Path { get; set; }

    string ApiUrl { get; set; }

    int CategoryId { get; set; }
}