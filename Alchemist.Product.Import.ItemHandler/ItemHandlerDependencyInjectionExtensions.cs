using Alchemist.Product.ImportItem.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Alchemist.Product.ImportItem.Handler;

public static class ItemHandlerDependencyInjectionExtensions
{
    public static IServiceCollection AddProductItemHandler(this IServiceCollection services)
    {
        return services.AddSingleton<IItemHandler<IImportProductItem>, ImportProductItemHandler>();
    }

    public static IServiceCollection AddCategoryItemHandler(this IServiceCollection services)
    {
        return services.AddSingleton<IItemHandler<IImportCategoryItem>, ImportCategoryItemHandler>();
    }
}
