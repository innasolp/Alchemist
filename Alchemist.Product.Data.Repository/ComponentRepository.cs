
namespace Alchemist.Product.Data.Repository;

public interface IComponentRepository:IRepository<Component, int> { }

public class ComponentRepository : RepositoryBase<Component, int>, IComponentRepository
{
    public ComponentRepository(AlchemyContext context) : base(context)
    {
    }

    protected override int GetId(Component entity)
    {
        return entity.Id;
    }
}
