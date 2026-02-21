using Autofac;
using Autofac.Extensions.DependencyInjection;
using BackgroundTaskQueue;
using Hangfire;
using Hangfire.Redis.StackExchange;
using Import.Service.Infrastructure;
using Import.Service.Infrastructure.Handlers;
using Mediator.Messages;
using MediatR;
using Message.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;

namespace ShopImport.Service.Hangfire;

public static class MediatrExtensions
{
    public static IHostBuilder AddImportServicesInfrastructure(this IHostBuilder hostBuilder, string hangfireConnectionString)
    {
        hostBuilder.ConfigureServices((context, services) =>
        {
            services.AddSingleton<IShopImportServiceManager, ShopImportServiceManager>();
            services.AddMediatR(cfg =>
            {
                cfg.RegisterGenericHandlers = true;

                cfg.RegisterServicesFromAssemblyContaining<AddShopImportServiceCommandHandler>();
            });

            var redis = ConnectionMultiplexer.Connect(hangfireConnectionString);
            services.AddHangfire(config =>
               config.UseRedisStorage(redis, new RedisStorageOptions
               {
                   Prefix = "hangfire-test:" // Optional: add a prefix to avoid key collisions
               }));

            // Добавляет и запускает Background Job Server
            services.AddHangfireServer(options => {
                options.WorkerCount = Environment.ProcessorCount * 5; // Настройка кол-ва воркеров
                options.Queues = ["default", "critical"];     // Очереди, которые слушает этот сервер
            });
        });

        hostBuilder.UseServiceProviderFactory(new AutofacServiceProviderFactory());
        return hostBuilder.ConfigureContainer<ContainerBuilder>((builderContext, builder) =>
        {
            builder.Register(c=>c.Resolve(typeof(IShopImportServiceManager))).As(typeof(IServiceManager));
            builder.Register(c => c.ResolveKeyed<IBackgroundTaskQueue>(ServiceKeys.EventBackgroundTaskQueue)).As<IBackgroundTaskQueue>();
            builder.Register(c => c.ResolveKeyed<IMessageSender>(ServiceKeys.EventMessageSenderKey)).As<IMessageSender>();
            builder.RegisterType(typeof(BackgroundMessageEventHandler<ServiceCreatedEvent, ServiceMessage>)).As(typeof(INotificationHandler<ServiceCreatedEvent>));
            builder.RegisterType(typeof(BackgroundMessageEventHandler<ServiceStartingEvent, ServiceMessage>)).As(typeof(INotificationHandler<ServiceStartingEvent>));
            builder.RegisterType(typeof(BackgroundMessageEventHandler<ServiceStartedEvent, ServiceStartedMessage>)).As(typeof(INotificationHandler<ServiceStartedEvent>));
            builder.RegisterType(typeof(BackgroundMessageEventHandler<ServiceStoppedEvent, ServiceMessage>)).As(typeof(INotificationHandler<ServiceStoppedEvent>));
        });
    }
}