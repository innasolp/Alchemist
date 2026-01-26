using Alchemist.DependencyInjection.Common;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using BackgroundTaskQueue;
using Import.Service.Commands.Handlers;
using Mediator.Messages;
using MediatR;
using MediatR.NotificationPublishers;
using Message.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Import.Service.Commands;

public static class MediatrExtensions
{
    public static IHostBuilder AddImportServicesInfrastructure(this IHostBuilder hostBuilder, string loggingCategory)
    {
        hostBuilder.ConfigureServices((context, services) =>
        {
            services.AddSingleton<IServiceRepository, ShopImportServiceRepository>();
            services.AddMediatR(cfg =>
            {
                cfg.RegisterGenericHandlers = true;

                cfg.NotificationPublisherType = typeof(LoggingNotificationPublisher<ForeachAwaitPublisher>);
                cfg.RegisterServicesFromAssemblyContaining<AddShopImportServiceCommandHandler>();
            });
        });

        hostBuilder.UseServiceProviderFactory(new AutofacServiceProviderFactory());
        return hostBuilder
            .ConfigureServices((hostContext, services) =>
            {
                services.AddLogger(loggingCategory);
            })
            .ConfigureContainer<ContainerBuilder>((builderContext, builder) =>
        {
            builder.Register(c => c.ResolveKeyed<IBackgroundTaskQueue>(ServiceKeys.EventBackgroundTaskQueue)).As<IBackgroundTaskQueue>();
            builder.Register(c => c.ResolveKeyed<IMessageSender>(ServiceKeys.EventMessageSenderKey)).As<IMessageSender>();
            builder.RegisterType(typeof(BackgroundMessageEventHandler<ServiceCreatedEvent, ServiceMessage>)).As(typeof(INotificationHandler<ServiceCreatedEvent>));
            builder.RegisterType(typeof(BackgroundMessageEventHandler<ServiceStartedEvent, ServiceMessage>)).As(typeof(INotificationHandler<ServiceStartedEvent>));
            builder.RegisterType(typeof(BackgroundMessageEventHandler<ServiceStoppedEvent, ServiceMessage>)).As(typeof(INotificationHandler<ServiceStoppedEvent>));
        });
    }
}