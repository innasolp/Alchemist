using Alchemist.Product.Data;
using Alchemist.Product.Data.Postgresql;
using Alchemist.Test.Server.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Alchemist.Product.Import.WebApp.Test;

public abstract class AlchemyDbAPIWebAppFactory<TStartup>(string dbConnectionString, bool ensureDeleted) : AlchemistDbContextWebAppFactory<TStartup, AlchemyContext>
    where TStartup : class
{
    private readonly string _dbConnectionString = dbConnectionString;
    private readonly bool _ensureDeleted = ensureDeleted;

    protected override IServiceCollection AddDbContext(IServiceCollection services)
    {
        return services.AddDbContextFactory<AlchemyContext, AlchemyContextPostgresFactory>(optionsBuilder => optionsBuilder.UseNpgsql(_dbConnectionString));
    }    

    protected override void ConfigureServiceProvider(IServiceProvider serviceProvider)
    {
        using var appContext = serviceProvider.GetRequiredService<AlchemyContext>();
        try
        {
            if(_ensureDeleted) appContext.Database.EnsureDeleted();
            appContext.Database.EnsureCreated();

            FillTestData(appContext);
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
}
