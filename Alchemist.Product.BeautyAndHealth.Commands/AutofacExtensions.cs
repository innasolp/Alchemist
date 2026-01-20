using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shop.Import.Common;

namespace Alchemist.Product.BeautyAndHealth.Commands;

public static class AutofacExtensions
{
    public static IHostBuilder AddBeautyAndHealthImportInfrastructure(this IHostBuilder hostBuilder)       
    {
        hostBuilder.ConfigureServices((context, services) =>
        {
            services.AddMediatR(cfg =>
            {
                cfg.RegisterGenericHandlers = true;
                cfg.RegisterServicesFromAssemblyContaining<ImportBeautyAndHealthProductCommandHandler>();
            });
        });

        hostBuilder.UseServiceProviderFactory(new AutofacServiceProviderFactory());
        return hostBuilder.ConfigureContainer<ContainerBuilder>((builderContext, builder) =>
        {
            builder.RegisterModule<ShopImportModule>();
        });
    }
}