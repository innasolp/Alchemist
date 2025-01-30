namespace Alchemist.Product.Data.Repository;

public interface IProductTypeRepository : IRepository<ProductType, short>
{
}

public class ProductTypeRepository : RepositoryBase<ProductType, short>, IProductTypeRepository
{
    public ProductTypeRepository(AlchemyContext context) : base(context)
    {
    }
    protected override short GetId(ProductType entity)
    {
        return entity.Id;
    }
}
