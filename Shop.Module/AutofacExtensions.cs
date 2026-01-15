using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Shop.Module;

public static class AutofacExtensions
{
    public static IHostBuilder AddProductInfrastructure(this IHostBuilder hostBuilder)
    {
        hostBuilder.UseServiceProviderFactory(new AutofacServiceProviderFactory());
        return hostBuilder.ConfigureContainer<ContainerBuilder>((builderContext, containerBuilder) =>
            {
                // Register your Autofac modules or components here
                containerBuilder.RegisterModule<ShopModule>();
            });

    }
}