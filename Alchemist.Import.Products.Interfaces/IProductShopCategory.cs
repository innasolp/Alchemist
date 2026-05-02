namespace Alchemist.Import.Products.Interfaces;

public interface IProductShopCategory
{
    string Category { get; set; }

    int ItemId { get; set; }

    public string Path { get; set; }
}