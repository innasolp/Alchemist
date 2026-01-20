using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Shop.Import.Common;

public static class AutofacExtensions
{
    public static IHostBuilder AddShopImportInfrastructure<T>(this IHostBuilder hostBuilder)//, params System.Reflection.Assembly[] assemblies)
        where T : Module, new()
    { 
        hostBuilder.UseServiceProviderFactory(new AutofacServiceProviderFactory());
        return hostBuilder.ConfigureContainer<ContainerBuilder>((builderContext, builder) =>
        {
            builder.RegisterModule<T>();
        });
    }
}