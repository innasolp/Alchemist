using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shop.Import.Common;

namespace Shop.Import.Category.Commands;

public static class AutofacExtensions
{
    public static IHostBuilder AddShopCategoryImportInfrastructure(this IHostBuilder hostBuilder)       
    {
        hostBuilder.ConfigureServices((context, services) =>
        {
            services.AddMediatR(cfg =>
            {
                cfg.RegisterGenericHandlers = true;
                cfg.RegisterServicesFromAssemblyContaining<ImportShopCategoryCommandHandler>();
            });
        });

        hostBuilder.UseServiceProviderFactory(new AutofacServiceProviderFactory());
        return hostBuilder.ConfigureContainer<ContainerBuilder>((builderContext, builder) =>
        {
            builder.RegisterModule<ShopImportModule>();
        });
    }
}