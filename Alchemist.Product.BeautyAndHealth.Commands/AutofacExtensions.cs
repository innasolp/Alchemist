using Alchemist.Common;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Db.Infrastructure;
using Microsoft.Extensions.Hosting;
using Shop.Import.Common;

namespace Alchemist.Product.BeautyAndHealth.Commands;

public static class AutofacExtensions
{
    public static IHostBuilder AddBeautyAndHealthImportInfrastructure(this IHostBuilder hostBuilder)       
    {  
        hostBuilder.UseServiceProviderFactory(new AutofacServiceProviderFactory());
        return hostBuilder.ConfigureContainer<ContainerBuilder>((builderContext, builder) =>
        {
            builder.RegisterModule<ShopImportModule>();

            builder.RegisterType(typeof(ImportBeautyAndHealthProductCommandHandler))
            .As(typeof(ICommandHandler<ImportBeautyAndHealthProductCommand, ItemProcessStatus>))
           .AsImplementedInterfaces().InstancePerLifetimeScope();

            builder.RegisterAssemblyTypes(typeof(ImportBeautyAndHealthProductCommandHandler).Assembly)
            .Where(t => t.Name.EndsWith("RequestHandler") || t.Name.EndsWith("CommandHandler"))
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();
        });
    }
}