using Autofac;
using Autofac.Extensions.DependencyInjection;
using BackgroundTaskQueue;
using Hangfire;
using Hangfire.Redis.StackExchange;
using Hangfire.Tags;
using Hangfire.Tags.Redis.StackExchange;
using Import.Service.Infrastructure;
using Import.Service.Infrastructure.Handlers;
using Mediator.Messages;
using MediatR;
using Message.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ShopImport.Service.Hangfire.Infrastructure;
using ShopImport.Service.Hangfire.Infrastructure.JobManagement;
using ShopImport.Service.Hangfire.Infrastructure.JobManagement.ChildJobStorages;
using ShopImport.Service.Hangfire.Infrastructure.PerformContextEnrichers;
using StackExchange.Redis;

namespace ShopImport.Service.Hangfire;

public static class MediatrExtensions
{
    private readonly static JobExecuteOptions defaultJobExecuteOptions = new() { ServerName = "DefaultServer", ProcessingQueue = "processing", WaitingQueue = "waiting" };

    public static IHostBuilder AddHangfireServiceManagementInfrastructure(this IHostBuilder hostBuilder, 
        string hangfireConnectionString,
        Action<DbContextOptionsBuilder> childStorageOptionsAction,
        JobExecuteOptions? jobExecuteOptions = null, 
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

            services.AddHangfireInfrastructure(hangfireConnectionString, jobExecuteOptions ?? defaultJobExecuteOptions, childStorageOptionsAction, configure);

            services.AddSingleton<IJobExecuteManager, JobExecuteManager>();
            services.AddSingleton<IImportServiceJobFactory, ShopImportServiceJobFactory>();
            services.AddSingleton<IPerformContextEnricher, ParentTagEnricher>();
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
        JobExecuteOptions jobExecuteOptions, 
        Action<DbContextOptionsBuilder> childStorageOptionsAction,
        Action<IGlobalConfiguration>? configure = null)
    {
        ClearRedisDataBase(hangfireConnectionString);

        services.AddHangfire((sp,config) =>
        {
            config.UseRedisStorage(hangfireConnectionString, new RedisStorageOptions
            {
                // Увеличьте этот таймаут, если задача длится дольше 30 минут
                InvisibilityTimeout = TimeSpan.FromHours(3)
            });

            config.UseTagsWithRedis(new TagsOptions { TagColor = "#1e8700" });

            config.UseFilter(new ChildTaskFilter(sp.GetRequiredService<IServiceScopeFactory>()));

            configure?.Invoke(config);           
                        
        });

        services.AddSingleton(jobExecuteOptions);

        services.AddDbContext<ChildJobDbContext>(childStorageOptionsAction);

        services.AddScoped<IChildJobStorage, EFChildJobStorage>();

        services.AddSingleton<ChildJobOrchestrator>();

        services.AddHangfireServer(options =>
        {
            options.Queues = [jobExecuteOptions.ProcessingQueue, "default"];
            options.WorkerCount = jobExecuteOptions.ParentWorkerCount;
            options.ServerName = jobExecuteOptions.ServerName;
        });

        services.AddHangfireServer(options =>
        {
            options.Queues = [jobExecuteOptions.ChildProcessingQueue];
            options.WorkerCount = jobExecuteOptions.ParentWorkerCount * jobExecuteOptions.ChildJobCountPerParent;
            options.ServerName = jobExecuteOptions.ChildServerName;
        });

        ThreadPool.SetMinThreads(100, 100);
    }

    public static void UseChildJobOrchestrator(this IHost host, JobExecuteOptions jobExecuteOptions)
    {
        ClearChildJobStorage(host);

        var recurringJobManager = host.Services.GetRequiredService<IRecurringJobManager>();

        recurringJobManager.AddOrUpdate<ChildJobOrchestrator>("child-orchestrator-tick",
            x => x.Dispatch(jobExecuteOptions.ChildJobCountPerParent,
                            jobExecuteOptions.ChildServerName,
                            jobExecuteOptions.ChildProcessingQueue),
            Cron.Minutely());        
    }

    private static void ClearChildJobStorage(IHost host)
    {
        using var scope = host.Services.CreateScope();
        var childJobDbContext = host.Services.GetRequiredService<ChildJobDbContext>();
        childJobDbContext.Database.EnsureDeleted();
        childJobDbContext.Database.EnsureCreated();
    }

    private static void ClearRedisDataBase(string hangfireConnectionString)
    {
        var redis = ConnectionMultiplexer.Connect($"{hangfireConnectionString},allowAdmin=true");

        var endpoints = redis.GetEndPoints();
        foreach (var endpoint in endpoints)
        {
            var server = redis.GetServer(endpoint);
            server.FlushDatabase();
        }
    }
}