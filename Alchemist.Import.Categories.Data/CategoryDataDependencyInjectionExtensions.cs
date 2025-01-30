using Microsoft.Extensions.DependencyInjection;

namespace Alchemist.Import.Categories.Data;

public static class CategoryDataDependencyInjectionExtensions
{
    public static IServiceCollection AddCategoriesDataHandler(this IServiceCollection services)
    {
        return services.AddTransient<ICategoryDataHandler, CategoriesDataHandler>();
    }
}
