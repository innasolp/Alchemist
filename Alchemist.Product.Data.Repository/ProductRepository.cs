namespace Alchemist.Product.Data.Repository;

public interface IProductRepository : IRepository<Product, long> { }

public class ProductRepository : RepositoryBase<Product, long>, IProductRepository
{
    public ProductRepository(AlchemyContext context) : base(context)
    {
    }

    protected override long GetId(Product entity)
    {
        return entity.Id;
    }
}
