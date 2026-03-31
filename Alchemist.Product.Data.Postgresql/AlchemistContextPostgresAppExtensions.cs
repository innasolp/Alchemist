using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Alchemist.Product.Data.Postgresql;

public static  class AlchemistContextPostgresAppExtensions
{
    public static void UseAlchemyPostgresqlMigration(this IHost app)
    {
        using var scope = app.Services.CreateScope();

        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AlchemyContext>>();

        using var context = factory.CreateDbContext();

        if (context.Database.CanConnect())
        {
            context.Database.Migrate();
        }
    }
}