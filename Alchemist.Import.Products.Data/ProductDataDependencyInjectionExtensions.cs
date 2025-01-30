using Microsoft.Extensions.DependencyInjection;

namespace Alchemist.Import.Products.Data;

public static class ProductDataDependencyInjectionExtensions
{
    public static IServiceCollection AddProductDataHandler(this IServiceCollection services)
    {
        return services.AddTransient<IProductDataHandler, ProductDataHandler>();
    }
}
