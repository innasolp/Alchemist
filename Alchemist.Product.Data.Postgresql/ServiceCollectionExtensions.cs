using Alchemist.Product.Data.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Alchemist.Product.Data.Postgresql;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAlchemyPostgresContextFactory(this IServiceCollection services, Action<DbContextOptionsBuilder>? optionsAction = null)
    {
        services.AddDbContextFactory<AlchemyContextPostgres>((serviceProvider, optionsBuilder) =>
            {
                optionsAction?.Invoke(optionsBuilder);

                optionsBuilder.AddInterceptors(serviceProvider.GetRequiredService<DateChangedInterceptor>(),
                    serviceProvider.GetRequiredService <MaterialPathInterceptor>());
            }
        );

        services.AddScoped<MaterialPathInterceptor>();
        services.AddScoped<DateChangedInterceptor>();

        return services.AddSingleton<IDbContextFactory<AlchemyContext>>
            (sp => new BaseContextFactoryAdapter<AlchemyContextPostgres, AlchemyContext>(sp.GetRequiredService<IDbContextFactory<AlchemyContextPostgres>>()));
    }
}