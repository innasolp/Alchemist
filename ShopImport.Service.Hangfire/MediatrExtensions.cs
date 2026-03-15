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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ShopImport.Service.Hangfire;
using ShopImport.Service.Hangfire.Infrastructure;
using ShopImport.Service.Hangfire.Infrastructure.JobExecutors;
using StackExchange.Redis;

namespace ShopImport.Service.Hangfire;

public static class MediatrExtensions
{
    private readonly static JobExecuteOptions defaultJobExecuteOptions = new() { ServerName = "DefaultServer", ProcessingQueue = "processing", WaitingQueue = "waiting" };

    public static IHostBuilder AddHangfireServiceManagementInfrastructure(this IHostBuilder hostBuilder, 
        string hangfireConnectionString,
        JobExecuteOptions? jobExecuteOptions = null, Action<IGlobalConfiguration>? configure = null)
    {
        hostBuilder.ConfigureServices((context, services) =>
        {
            services.AddSingleton<IShopImportServiceJobManager, HangfireShopImportServiceManager>();
            services.AddMediatR(cfg =>
            {
                cfg.RegisterGenericHandlers = true;

                cfg.RegisterServicesFromAssemblyContaining<AddShopImportServiceCommandHandler>();
            });

            services.AddHangfire(hangfireConnectionString, jobExecuteOptions ?? defaultJobExecuteOptions, configure);

            services.AddSingleton<IJobExecuteManager, JobExecuteManager>();
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

    private static void AddHangfire(this IServiceCollection services, string hangfireConnectionString, JobExecuteOptions jobExecuteOptions, Action<IGlobalConfiguration>? configure = null)
    {
        ClearRedisDataBase(hangfireConnectionString);

        services.AddHangfire(config =>
        {
            config.UseRedisStorage(hangfireConnectionString, new RedisStorageOptions
            {
                // Увеличьте этот таймаут, если задача длится дольше 30 минут
                InvisibilityTimeout = TimeSpan.FromHours(3)
            });

            config.UseTagsWithRedis(new TagsOptions { TagColor = "#1e8700"});

            configure?.Invoke(config);
        });

        services.AddSingleton(jobExecuteOptions);        

        services.AddHangfireServer(options =>
        {
            options.Queues = [jobExecuteOptions.ProcessingQueue, "default"];
            options.WorkerCount = 20; //todo
            options.ServerName = jobExecuteOptions.ServerName;
        });

        ThreadPool.SetMinThreads(100, 100);
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