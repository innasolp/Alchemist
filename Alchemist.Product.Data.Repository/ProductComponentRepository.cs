
namespace Alchemist.Product.Data.Repository;

public interface IProductComponentRepository : IRepository<ProductComponent, KeyValuePair<long, int>> { }

public class ProductComponentRepository : RepositoryBase<ProductComponent, KeyValuePair<long, int>>, IProductComponentRepository
{
    public ProductComponentRepository(AlchemyContext context) : base(context)
    {
    }

    protected override KeyValuePair<long, int> GetId(ProductComponent entity)
    {
        return new KeyValuePair<long, int>(entity.ProductId, entity.ComponentId);
    }
}
