namespace Alchemist.Product.Data.Repository;

public interface ICountryRepository : IRepository<Country, short> { }

public class CountryRepository : RepositoryBase<Country, short>, ICountryRepository
{
    public CountryRepository(AlchemyContext context) : base(context)
    {
    }

    protected override short GetId(Country entity)
    {
        return entity.Id;
    }
}
