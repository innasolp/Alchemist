namespace Alchemist.Product.Data.Repository;

public interface IBrandRepository : IRepository<Brand, int> { }

public class BrandRepository : RepositoryBase<Brand, int>, IBrandRepository
{
    public BrandRepository(AlchemyContext context) : base(context)
    {
    }

    protected override int GetId(Brand entity)
    {
        return entity.Id;
    }
}
