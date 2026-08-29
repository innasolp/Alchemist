using Alchemist.Common;
using Alchemist.Product.CategoryData;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Db.Infrastructure;
using Microsoft.Extensions.Hosting;
using Shop.Import.Common;

namespace Shop.Import.Category.Commands;

public static class AutofacExtensions
{
    public static IHostBuilder AddShopCategoryImportInfrastructure(this IHostBuilder hostBuilder)       
    {
        hostBuilder.UseServiceProviderFactory(new AutofacServiceProviderFactory());
        return hostBuilder.ConfigureContainer<ContainerBuilder>((builderContext, builder) =>
        {
            builder.RegisterModule<ShopImportModule>();

            var assembly = typeof(ImportShopCategoryCommandHandler).Assembly;

            builder.RegisterAssemblyTypes(assembly)
                .AsClosedTypesOf(typeof(ICommandHandler<,>))
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            builder.RegisterAssemblyTypes(assembly)
            .Where(t => t.Name.EndsWith("RequestHandler") || t.Name.EndsWith("CommandHandler"))
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();

        });
    }
}