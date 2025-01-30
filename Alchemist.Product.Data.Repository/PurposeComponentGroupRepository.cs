namespace Alchemist.Product.Data.Repository;

public interface IPurposeComponentGroupRepository : IRepository<PurposeComponentGroup, KeyValuePair<short, short>> { }

public class PurposeComponentGroupRepository : RepositoryBase<PurposeComponentGroup, KeyValuePair<short, short>>, IPurposeComponentGroupRepository
{
    public PurposeComponentGroupRepository(AlchemyContext context) : base(context)
    {
    }

    protected override KeyValuePair<short, short> GetId(PurposeComponentGroup entity)
    {
        return new KeyValuePair<short, short>(entity.ComponentGroupId, entity.PurposeTypeId);
    }

    public override PurposeComponentGroup Get(KeyValuePair<short, short> id)
    {
        return Context.Set<PurposeComponentGroup>().FirstOrDefault(m => m.ComponentGroupId == id.Key && m.PurposeTypeId == id.Value);
    }
}
