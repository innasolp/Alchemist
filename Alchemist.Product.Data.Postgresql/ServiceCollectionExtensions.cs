using Alchemist.Product.Data.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Alchemist.Product.Data.Postgresql;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAlchemyPostgresContextFactory(this IServiceCollection services, Action<DbContextOptionsBuilder>? optionsAction = null)
    {
        services.AddDbContextFactory<AlchemyContextPostgres>(optionsBuilder =>
        {
            optionsAction?.Invoke(optionsBuilder);

            optionsBuilder.AddInterceptors(new DateChangedInterceptor());
        });

        return services.AddSingleton<IDbContextFactory<AlchemyContext>>
            (sp => new BaseContextFactoryAdapter<AlchemyContextPostgres, AlchemyContext>(sp.GetRequiredService<IDbContextFactory<AlchemyContextPostgres>>()));
    }
}