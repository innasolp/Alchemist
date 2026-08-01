using Autofac;
using Autofac.Extensions.DependencyInjection;
using BackgroundTaskQueue;
using Db.Infrastructure;
using Db.Infrastructure.Messages;
using Hangfire;
using Hangfire.AggregateJobs;
using Import.Service.Infrastructure;
using Import.Service.Infrastructure.Handlers;
using Message.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ShopImport.Service.Hangfire.Infrastructure;
using ShopImport.Service.Hangfire.Infrastructure.Enrichers;
using ShopImport.Service.Hangfire.Infrastructure.Filters;

namespace ShopImport.Service.Hangfire;

public static class AutofacExtensions
{
    private readonly static AggregateServerSettings defaultAggregateServerSettings =
        new() { ServerName = "DefaultServer", ProcessingQueue = "processing", WaitingQueue = "waiting" };

    private static IHostBuilder AddHangfireServiceManagementInfrastructure(this IHostBuilder hostBuilder,
        Func<HostBuilderContext, string> getHangfireConnectionString,
        Action<IGlobalConfiguration, string> configureHangfireStorage,
        Action<DbContextOptionsBuilder> childStorageOptionsAction,
        AggregateServerSettings? aggregateServerSettings = null,
        Action<IGlobalConfiguration>? configure = null)
    {
        hostBuilder.ConfigureServices((context, services) =>
        {
            services.AddSingleton<IShopImportServiceJobManager, HangfireShopImportServiceManager>();

            services.AddHangfireInfrastructure(getHangfireConnectionString(context),
                aggregateServerSettings ?? defaultAggregateServerSettings,
                configureHangfireStorage,
                childStorageOptionsAction,
                configure);

            services.AddSingleton<IImportServiceJobFactory, ShopImportServiceJobFactory>();
        });

        hostBuilder.UseServiceProviderFactory(new AutofacServiceProviderFactory());

        return hostBuilder.ConfigureContainer<ContainerBuilder>((builderContext, builder) =>
        {
            builder.RegisterAssemblyTypes(typeof(AddShopImportServiceCommandHandler).Assembly)
            .Where(t => t.Name.EndsWith("CommandHandler"))
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();

            builder.Register(c => c.Resolve(typeof(IShopImportServiceJobManager))).As(typeof(IShopImportServiceManager)).SingleInstance();
            builder.Register(c => c.Resolve(typeof(IShopImportServiceJobManager))).As(typeof(IServiceManager)).SingleInstance();
            builder.Register(c => c.Resolve(typeof(IShopImportServiceJobManager))).As(typeof(IHagfireServiceJobManager)).SingleInstance();

            builder.Register(c => c.ResolveKeyed<IBackgroundTaskQueue>(ServiceKeys.EventBackgroundTaskQueue)).As<IBackgroundTaskQueue>();
            builder.Register(c => c.ResolveKeyed<IMessageSender>(ServiceKeys.EventMessageSenderKey)).As<IMessageSender>();

            builder.RegisterType(typeof(EntityEventPublisher)).As(typeof(IEntityEventPublisher));

            builder.AddBackgroundMessageHandler<ServiceCreatedEvent, ServiceMessage>();
            builder.AddBackgroundMessageHandler<ServiceStartingEvent, ServiceMessage>();
            builder.AddBackgroundMessageHandler<ServiceStartedEvent, ServiceStartedMessage>();
            builder.AddBackgroundMessageHandler<ServiceStoppedEvent, ServiceMessage>();

        });
    }

    public static IHostBuilder AddHangfireServiceManagementInfrastructure(this IHostBuilder hostBuilder,
        string hangfireConnectionString,
        Action<IGlobalConfiguration, string> configureHangfireStorage,
        Action<DbContextOptionsBuilder> childStorageOptionsAction,
        AggregateServerSettings? aggregateServerSettings = null,
        Action<IGlobalConfiguration>? configure = null)
    {
        return hostBuilder.AddHangfireServiceManagementInfrastructure((context) => hangfireConnectionString,
            configureHangfireStorage,
            childStorageOptionsAction,
            aggregateServerSettings,
            configure);
    }

    public static IHostBuilder AddHangfireServiceManagementInfrastructureFromContext(this IHostBuilder hostBuilder,
        string hangfireConnectionSection,
        Action<IGlobalConfiguration, string> configureHangfireStorage,
        Action<DbContextOptionsBuilder> childStorageOptionsAction,
        AggregateServerSettings? aggregateServerSettings = null,
        Action<IGlobalConfiguration>? configure = null)
    {
        return hostBuilder.AddHangfireServiceManagementInfrastructure((context) => context.Configuration.GetConnectionString(hangfireConnectionSection),
            configureHangfireStorage,
            childStorageOptionsAction,
            aggregateServerSettings,
            configure);
    }

    private static void AddHangfireInfrastructure(this IServiceCollection services,
        string hangfireConnectionString,
        AggregateServerSettings aggregateServerSettings,
        Action<IGlobalConfiguration, string> configureHangfireStorage,
        Action<DbContextOptionsBuilder> childStorageOptionsAction,
        Action<IGlobalConfiguration>? configure = null)
    {
        services.AddSingleton(aggregateServerSettings);

        void importConfigure(IGlobalConfiguration config)
        {
            configure?.Invoke(config);

            config.UseFilter(new DeletedStateFilter());
        }

        services.AddHangfireAggreateJobs<IHagfireServiceJobManager>(hangfireConnectionString,
            aggregateServerSettings,
            childStorageOptionsAction,
            configureHangfireStorage,
            importConfigure
            );

        services.AddIddleJobClenUp();

        services.AddExpiredJobCleanUp();

        services.AddScoped<IChildJobEnricher<IImportServiceJob>, ParentJobTagEnricher>();

        ThreadPool.SetMinThreads(100, 100);
    }

    public static void UseImportServiceChildJobOrchestrator(this IHost host, AggregateServerSettings aggregateServerSettings)
    {
        host.UseChildJobOrchestrator<IHagfireServiceJobManager>(aggregateServerSettings);

        host.UseIddleJobCleanUp();

        host.UseExpiredJobCleanUp();
    }
}