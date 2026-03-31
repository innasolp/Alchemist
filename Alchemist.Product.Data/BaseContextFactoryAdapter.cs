using Microsoft.EntityFrameworkCore;

namespace Alchemist.Product.Data;

public class BaseContextFactoryAdapter<TContext, TBaseContext>(IDbContextFactory<TContext> factory) : IDbContextFactory<TBaseContext>
    where TBaseContext : DbContext
    where TContext : TBaseContext
{
    private readonly IDbContextFactory<TContext> _factory = factory;

    public TBaseContext CreateDbContext() => _factory.CreateDbContext();
}