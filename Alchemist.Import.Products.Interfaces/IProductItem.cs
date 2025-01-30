namespace Alchemist.Import.Products.Interfaces;

public interface IProductItem
{
    string ItemId { get; }

    string Name { get; }

    string Shop { get; }

    string[] Components { get; }

    string Brand { get; }

    string Country { get; }

    string Comment { get; }

    string ProductType { get; }

    string[] Purposes { get; }

    string Articul { get; }

    string Currency { get; set; }

    double Price { get; set; }

    string ItemUrl { get; set; }

    string ApiUrl { get; set; }
}
