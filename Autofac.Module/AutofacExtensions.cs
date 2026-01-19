using Autofac;
using Autofac.Extensions.DependencyInjection;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Mediator.Module.EF;

public static class AutofacExtensions
{
    public static IHostBuilder AddMediatorInfrastructure<T>(this IHostBuilder hostBuilder)//, params System.Reflection.Assembly[] assemblies)
        where T : MediatorModule, new()
    {
        var module = new T();

        hostBuilder.ConfigureServices((context, services) =>
        {
            services.AddMediatR(cfg =>
            {
                cfg.RegisterGenericHandlers = true;

                module.ConfigureMediator(cfg);
            });
        });

        hostBuilder.UseServiceProviderFactory(new AutofacServiceProviderFactory());
        return hostBuilder.ConfigureContainer<ContainerBuilder>((builderContext, builder) =>
        {
            builder.RegisterModule(module);
        });
    }
}