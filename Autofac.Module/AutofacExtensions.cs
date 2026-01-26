using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Mediator.Module.EF;

public static class AutofacExtensions
{
    public static IHostBuilder AddMediatorInfrastructure<T>(this IHostBuilder hostBuilder)
        where T : MediatorModule, new()
    {
        var module = new T();

        return hostBuilder.AddMediatorInfrastructure(module);
    }

    public static IHostBuilder AddMediatorInfrastructure(this IHostBuilder hostBuilder, MediatorModule module)
    {
        hostBuilder.ConfigureServices((context, services) =>
        {
            module.ConfigureServices(services); 
        });

        hostBuilder.UseServiceProviderFactory(new AutofacServiceProviderFactory());
        return hostBuilder.ConfigureContainer<ContainerBuilder>((builderContext, builder) =>
        {
            builder.RegisterModule(module);
        });
    }
}