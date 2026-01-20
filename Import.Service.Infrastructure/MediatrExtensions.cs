using Autofac;
using Autofac.Extensions.DependencyInjection;
using Import.Service.Commands.Handlers;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Import.Service.Commands;

public static class MediatrExtensions
{
    public static IHostBuilder AddImportServicesInfrastructure(this IHostBuilder hostBuilder)
    {
        hostBuilder.ConfigureServices((context, services) =>
        {
            services.AddSingleton<IServiceRepository, ShopImportServiceRepository>();
            services.AddMediatR(cfg =>
            {
                cfg.RegisterGenericHandlers = true;
                cfg.RegisterServicesFromAssemblyContaining<AddShopImportServiceCommandHandler>();
            });
        });

        hostBuilder.UseServiceProviderFactory(new AutofacServiceProviderFactory());
        return hostBuilder.ConfigureContainer<ContainerBuilder>((builderContext, builder) =>
        {
            builder.RegisterType(typeof(MessageEventHandler<ServiceCreatedEvent, ServiceMessage>)).As(typeof(INotificationHandler<ServiceCreatedEvent>));
            builder.RegisterType(typeof(MessageEventHandler<ServiceStartedEvent, ServiceMessage>)).As(typeof(INotificationHandler<ServiceStartedEvent>));
            builder.RegisterType(typeof(MessageEventHandler<ServiceStoppedEvent, ServiceMessage>)).As(typeof(INotificationHandler<ServiceStoppedEvent>));
        });
    }
}