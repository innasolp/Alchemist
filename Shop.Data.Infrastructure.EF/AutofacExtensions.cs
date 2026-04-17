using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Shop.Data.Infrastructure.EF;

public static class AutofacExtensions
{
    public static IHostBuilder AddShopInfrastructure(this IHostBuilder hostBuilder)        
    {
        var module = new ShopDataModule();

        return hostBuilder.AddShopInfrastructure(module);
    }

    public static IHostBuilder AddShopInfrastructure(this IHostBuilder hostBuilder, ShopDataModule module)
    {
        hostBuilder.UseServiceProviderFactory(new AutofacServiceProviderFactory());
        return hostBuilder.ConfigureContainer<ContainerBuilder>((builderContext, builder) =>
        {
            builder.RegisterModule(module);
        });
    }
}