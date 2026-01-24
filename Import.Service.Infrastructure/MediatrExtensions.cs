using Autofac;
using Autofac.Extensions.DependencyInjection;
using Import.Service.Commands.Handlers;
using Mediator.Messages;
using MediatR;
using MediatR.NotificationPublishers;
using Message.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
                services.AddScoped<INotificationPublisher, LoggingNotificationPublisher<ForeachAwaitPublisher>>(serviceProvider =>
                {
                    var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
                    var logger = loggerFactory.CreateLogger(loggingCategory);
                    return new LoggingNotificationPublisher<ForeachAwaitPublisher>(logger);
                });
            })
            .ConfigureContainer<ContainerBuilder>((builderContext, builder) =>
        {
            builder.Register(c => c.ResolveKeyed<IMessageSender>(ServiceKeys.EventMessageSenderKey)).As<IMessageSender>();
            builder.RegisterType(typeof(MessageEventHandler<ServiceCreatedEvent, ServiceMessage>)).As(typeof(INotificationHandler<ServiceCreatedEvent>));
            builder.RegisterType(typeof(MessageEventHandler<ServiceStartedEvent, ServiceMessage>)).As(typeof(INotificationHandler<ServiceStartedEvent>));
            builder.RegisterType(typeof(MessageEventHandler<ServiceStoppedEvent, ServiceMessage>)).As(typeof(INotificationHandler<ServiceStoppedEvent>));
        });
    }
}