using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Product.Data.Infrastructure.EF;

public static class AutofacExtensions
{
    public static IHostBuilder AddProductInfrastructure(this IHostBuilder hostBuilder, Action<ContainerBuilder>? optionsAction = null)        
    {
        var module = new ProductDataModule();

        return hostBuilder.AddProductInfrastructure(module, optionsAction);
    }

    public static IHostBuilder AddProductInfrastructure(this IHostBuilder hostBuilder, ProductDataModule module, Action<ContainerBuilder>? optionsAction = null)
    {
        hostBuilder.UseServiceProviderFactory(new AutofacServiceProviderFactory());

        return hostBuilder.ConfigureContainer<ContainerBuilder>((builderContext, builder) =>
        {
            builder.RegisterModule(module);

            optionsAction?.Invoke(builder);
        });
    }
}