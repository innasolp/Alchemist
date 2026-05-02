namespace Alchemist.Product.Infrastructure.Interfaces;

public interface IProductRepository
{ 
    Task<Data.Product?> FindProductByNameAndBrand(string name, string brand, CancellationToken cancellationToken = default);  
}
