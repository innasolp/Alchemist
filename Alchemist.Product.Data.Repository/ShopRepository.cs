
namespace Alchemist.Product.Data.Repository;

public interface IShopRepository : IRepository<Shop, int> { }

public class ShopRepository : RepositoryBase<Shop, int>, IShopRepository
{
    public ShopRepository(AlchemyContext context) : base(context)
    {
    }

    protected override int GetId(Shop entity)
    {
       return  entity.Id;
    }
}
