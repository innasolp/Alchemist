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

            builder.RegisterType(typeof(ImportShopCategoryCommandHandler))
            .As(typeof(ICommandHandler<ImportShopCategoryCommand, ItemProcessStatus>))
           .AsImplementedInterfaces().InstancePerLifetimeScope();

            builder.RegisterAssemblyTypes(typeof(ImportShopCategoryCommandHandler).Assembly)
            .Where(t => t.Name.EndsWith("RequestHandler") || t.Name.EndsWith("CommandHandler"))
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();

        });
    }
}