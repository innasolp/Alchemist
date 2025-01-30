namespace Alchemist.Product.Data.Repository;

public interface IShopProductRepository : IRepository<ShopProduct, KeyValuePair<int, long>>
{ }

public class ShopProductRepository : RepositoryBase<ShopProduct, KeyValuePair<int, long>>, IShopProductRepository
{
    public ShopProductRepository(AlchemyContext context) : base(context)
    {
    }

    protected override KeyValuePair<int, long> GetId(ShopProduct entity)
    {
        return new KeyValuePair<int, long>(entity.ShopId, entity.ProductId);
    }
}
