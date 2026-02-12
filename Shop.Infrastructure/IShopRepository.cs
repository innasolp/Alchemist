namespace Shop.Infrastructure;

public interface IShopRepository 
{
    Task<Alchemist.Product.Data.Shop?> GetShopByUrl(string url, CancellationToken cancellationToken = default);
}