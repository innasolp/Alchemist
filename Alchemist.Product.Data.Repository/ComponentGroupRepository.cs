
namespace Alchemist.Product.Data.Repository;

public interface IComponentGroupRepository : IRepository<ComponentGroup, int> { }

public class ComponentGroupRepository : RepositoryBase<ComponentGroup, int>, IComponentGroupRepository
{
    public ComponentGroupRepository(AlchemyContext context) : base(context)
    {
    }

    protected override int GetId(ComponentGroup entity)
    {
        return entity.Id;
    }
}
