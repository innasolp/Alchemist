namespace Alchemist.Product.Interfaces;

public interface IShopProductPrice
{
    long Id { get; set; }

    long ShopProductId { get; set; }

    double Price { get; set; }
    
    int CurrencyId { get; set; }
}
