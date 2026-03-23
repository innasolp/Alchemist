using Autofac;
using Autofac.Extensions.DependencyInjection;
using BackgroundTaskQueue;
using Hangfire;
using Hangfire.AggregateJobs;
using Import.Service.Infrastructure;
using Import.Service.Infrastructure.Handlers;
using Mediator.Messages;
using MediatR;
using Message.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ShopImport.Service.Hangfire.Infrastructure;
using ShopImport.Service.Hangfire.Infrastructure.PerformContextEnrichers;

namespace ShopImport.Service.Hangfire;

public static class MediatrExtensions
{
    private readonly static AggregateServerSettings defaultAggregateServerSettings =
        new() { ServerName = "DefaultServer", ProcessingQueue = "processing", WaitingQueue = "waiting" };

    public static IHostBuilder AddHangfireServiceManagementInfrastructure(this IHostBuilder hostBuilder, 
        string hangfireConnectionString,
        Action<IGlobalConfiguration, string> configureHangfireStorage,
        Action<DbContextOptionsBuilder> childStorageOptionsAction,
        AggregateServerSettings? aggregateServerSettings = null, 
        Action<IGlobalConfiguration>? configure = null)
    {
        hostBuilder.ConfigureServices((context, services) =>
        {
            services.AddSingleton<IShopImportServiceJobManager, HangfireShopImportServiceManager>();
            services.AddMediatR(cfg =>
            {
                cfg.RegisterGenericHandlers = true;

                cfg.RegisterServicesFromAssemblyContaining<AddShopImportServiceCommandHandler>();
            });

            services.AddHangfireInfrastructure(hangfireConnectionString, 
                aggregateServerSettings ?? defaultAggregateServerSettings,
                configureHangfireStorage,
                childStorageOptionsAction, 
                configure);

            services.AddSingleton<IImportServiceJobFactory, ShopImportServiceJobFactory>();            
        });

        hostBuilder.UseServiceProviderFactory(new AutofacServiceProviderFactory());
        return hostBuilder.ConfigureContainer<ContainerBuilder>((builderContext, builder) =>
        {
            builder.Register(c=>c.Resolve(typeof(IShopImportServiceJobManager))).As(typeof(IShopImportServiceManager));
            builder.Register(c=>c.Resolve(typeof(IShopImportServiceJobManager))).As(typeof(IServiceManager));
            builder.Register(c=>c.Resolve(typeof(IShopImportServiceJobManager))).As(typeof(IHagfireServiceJobManager));            

            builder.Register(c => c.ResolveKeyed<IBackgroundTaskQueue>(ServiceKeys.EventBackgroundTaskQueue)).As<IBackgroundTaskQueue>();
            builder.Register(c => c.ResolveKeyed<IMessageSender>(ServiceKeys.EventMessageSenderKey)).As<IMessageSender>();

            builder.RegisterType(typeof(BackgroundMessageEventHandler<ServiceCreatedEvent, ServiceMessage>)).As(typeof(INotificationHandler<ServiceCreatedEvent>));
            builder.RegisterType(typeof(BackgroundMessageEventHandler<ServiceStartingEvent, ServiceMessage>)).As(typeof(INotificationHandler<ServiceStartingEvent>));
            builder.RegisterType(typeof(BackgroundMessageEventHandler<ServiceStartedEvent, ServiceStartedMessage>)).As(typeof(INotificationHandler<ServiceStartedEvent>));
            builder.RegisterType(typeof(BackgroundMessageEventHandler<ServiceStoppedEvent, ServiceMessage>)).As(typeof(INotificationHandler<ServiceStoppedEvent>));
        });
    }

    private static void AddHangfireInfrastructure(this IServiceCollection services,
        string hangfireConnectionString,
        AggregateServerSettings aggregateServerSettings,
        Action<IGlobalConfiguration, string> configureHangfireStorage,
        Action<DbContextOptionsBuilder> childStorageOptionsAction,
        Action<IGlobalConfiguration>? configure = null)
    {
        services.AddSingleton(aggregateServerSettings);

        services.AddHangfireAggreateJobs<IHagfireServiceJobManager>(hangfireConnectionString,
            aggregateServerSettings,
            childStorageOptionsAction,
            configureHangfireStorage,
        configure);

        services.AddScoped<IChildJobEnricher<IImportServiceJob>, ParentJobTagEnricher>();

        ThreadPool.SetMinThreads(100, 100);
    }

    public static void UseImportServiceChildJobOrchestrator(this IHost host, AggregateServerSettings aggregateServerSettings)
    {
        host.UseChildJobOrchestrator<IHagfireServiceJobManager>(aggregateServerSettings);
        host.ClearChildJobStorage();
    }    
}