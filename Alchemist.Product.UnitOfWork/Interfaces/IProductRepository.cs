using Alchemist.Product.Data;
using UnitOfWork;

namespace Alchemist.Product.UnitOfWork.Interfaces;

public interface IProductRepository : IRepository<Data.Product>
{ 
    Task<Data.Product?> FindProductByNameAndBrand(string name, string brand, CancellationToken cancellationToken = default);  
}
