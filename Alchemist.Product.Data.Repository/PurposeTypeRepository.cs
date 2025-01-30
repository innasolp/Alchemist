
namespace Alchemist.Product.Data.Repository;

public interface IPurposeTypeRepository : IRepository<PurposeType, short> { }

public class PurposeTypeRepository : RepositoryBase<PurposeType, short>, IPurposeTypeRepository
{
    public PurposeTypeRepository(AlchemyContext context) : base(context)
    {
    }

    protected override short GetId(PurposeType entity)
    {
        return entity.Id;
    }
}
