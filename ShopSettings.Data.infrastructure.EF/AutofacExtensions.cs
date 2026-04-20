using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ShopSettings.Data.infrastructure.EF;

public static class AutofacExtensions
{
    public static IHostBuilder AddShopSettingsInfrastructure(this IHostBuilder hostBuilder, Action<ContainerBuilder>? optionsAction = null)        
    {
        var module = new ShopDataSetingsModule();

        return hostBuilder.AddShopSettingsInfrastructure(module, optionsAction);
    }

    public static IHostBuilder AddShopSettingsInfrastructure(this IHostBuilder hostBuilder, ShopDataSetingsModule module, Action<ContainerBuilder>? optionsAction = null)
    {
        hostBuilder.UseServiceProviderFactory(new AutofacServiceProviderFactory());

        return hostBuilder.ConfigureContainer<ContainerBuilder>((builderContext, builder) =>
        {
            builder.RegisterModule(module);

            optionsAction?.Invoke(builder);
        });
    }
}