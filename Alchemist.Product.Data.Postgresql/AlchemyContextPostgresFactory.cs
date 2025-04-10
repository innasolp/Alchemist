using Microsoft.EntityFrameworkCore;

namespace Alchemist.Product.Data.Postgresql;

public class AlchemyContextPostgresFactory : IDbContextFactory<AlchemyContext>
{
    public AlchemyContext CreateDbContext()
    {
        return new AlchemyContextPostgres();
    }
}
